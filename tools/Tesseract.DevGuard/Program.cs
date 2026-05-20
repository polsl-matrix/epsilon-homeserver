using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

return await CommandDispatcher.RunAsync(args);

static class CommandDispatcher
{
    private const string TicketPrefix = "ES";

    public static async Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        return args[0] switch
        {
            "check-gitleaks" => await CheckGitleaksAsync(),
            "validate-commit-msg" => await ValidateCommitMessageAsync(args.Skip(1).ToArray()),
            "validate-push-branch" => await ValidatePushBranchAsync(),
            _ => UnknownCommand(args[0])
        };
    }

    private static async Task<int> CheckGitleaksAsync()
    {
        var result = await ProcessRunner.TryRunAsync("gitleaks", ["version"]);
        if (result.ExitCode == 0)
        {
            return 0;
        }

        Console.Error.WriteLine("gitleaks not found. Install it before committing.");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Console.Error.WriteLine("macOS: brew install gitleaks");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.Error.WriteLine("Windows (Chocolatey): choco install gitleaks -y");
            Console.Error.WriteLine("Windows (manual): download release from https://github.com/gitleaks/gitleaks/releases and add binary to PATH.");
        }
        else
        {
            Console.Error.WriteLine("Linux: download gitleaks release or install via your package manager.");
        }

        return 1;
    }

    private static async Task<int> ValidateCommitMessageAsync(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Missing commit message file path.");
            return 1;
        }

        var commitMessageFilePath = args[0];
        var rawCommitMessage = await File.ReadAllTextAsync(commitMessageFilePath);
        var commitMessage = rawCommitMessage.Trim();
        if (string.IsNullOrWhiteSpace(commitMessage) || IsBypassMessage(commitMessage))
        {
            return 0;
        }

        var branchName = await Git.GetCurrentBranchAsync();
        var ticket = ExtractTicket(branchName);
        if (ticket is null)
        {
            return 0;
        }

        var expectedPrefix = $"[{ticket}] ";
        if (commitMessage.StartsWith(expectedPrefix, StringComparison.Ordinal))
        {
            return 0;
        }

        var updatedCommitMessage = PrefixCommitMessage(rawCommitMessage, expectedPrefix);
        await File.WriteAllTextAsync(commitMessageFilePath, updatedCommitMessage);

        Console.WriteLine($"Added missing commit prefix '{expectedPrefix}' from branch '{branchName}'.");
        return 0;
    }

    private static async Task<int> ValidatePushBranchAsync()
    {
        var branchName = await Git.GetCurrentBranchAsync();
        if (!string.Equals(branchName, "main", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(branchName, "master", StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        Console.Error.WriteLine($"Push blocked for protected branch '{branchName}'. Open pull request instead.");
        return 1;
    }

    private static string? ExtractTicket(string branchName)
    {
        var jiraStyleMatch = Regex.Match(branchName, @"\b([A-Za-z]{2,}-\d+)\b");
        if (jiraStyleMatch.Success)
        {
            return jiraStyleMatch.Groups[1].Value.ToUpperInvariant();
        }

        foreach (var segment in branchName.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var numericBranchMatch = Regex.Match(segment, @"^(?<id>\d+)(?:[-_].+)?$");
            if (numericBranchMatch.Success)
            {
                return $"{TicketPrefix}-{numericBranchMatch.Groups["id"].Value}";
            }
        }

        return null;
    }

    private static bool IsBypassMessage(string commitMessage) =>
        commitMessage.StartsWith("Merge ", StringComparison.OrdinalIgnoreCase) ||
        commitMessage.StartsWith("Revert ", StringComparison.OrdinalIgnoreCase) ||
        commitMessage.StartsWith("fixup!", StringComparison.OrdinalIgnoreCase) ||
        commitMessage.StartsWith("squash!", StringComparison.OrdinalIgnoreCase);

    private static string PrefixCommitMessage(string rawCommitMessage, string expectedPrefix)
    {
        var trimmedStart = rawCommitMessage.TrimStart();
        var leadingWhitespaceLength = rawCommitMessage.Length - trimmedStart.Length;
        var leadingWhitespace = rawCommitMessage[..leadingWhitespaceLength];
        var normalizedMessage = trimmedStart.TrimEnd('\r', '\n');
        var trailingNewLine = rawCommitMessage.EndsWith("\r\n", StringComparison.Ordinal)
            ? "\r\n"
            : rawCommitMessage.EndsWith("\n", StringComparison.Ordinal)
                ? "\n"
                : string.Empty;

        return $"{leadingWhitespace}{expectedPrefix}{normalizedMessage}{trailingNewLine}";
    }

    private static int UnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command '{command}'.");
        PrintUsage();
        return 1;
    }

    private static void PrintUsage() =>
        Console.Error.WriteLine("Usage: check-gitleaks | validate-commit-msg <path> | validate-push-branch");
}

static class Git
{
    public static async Task<string> GetCurrentBranchAsync()
    {
        var overriddenBranch = Environment.GetEnvironmentVariable("DEVGUARD_BRANCH_NAME");
        if (!string.IsNullOrWhiteSpace(overriddenBranch))
        {
            return overriddenBranch.Trim();
        }

        var result = await ProcessRunner.RunAsync("git", ["branch", "--show-current"]);
        return result.StandardOutput.Trim();
    }
}

static class ProcessRunner
{
    public static Task<CommandResult> TryRunAsync(string fileName, IReadOnlyList<string> arguments) =>
        RunCoreAsync(fileName, arguments, throwOnFailureToStart: false);

    public static async Task<CommandResult> RunAsync(string fileName, IReadOnlyList<string> arguments)
    {
        var result = await RunCoreAsync(fileName, arguments, throwOnFailureToStart: true);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(result.StandardError.Length > 0
                ? result.StandardError.Trim()
                : $"Command '{fileName}' exited with code {result.ExitCode}.");
        }

        return result;
    }

    private static async Task<CommandResult> RunCoreAsync(string fileName, IReadOnlyList<string> arguments, bool throwOnFailureToStart)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        try
        {
            using var process = new Process { StartInfo = startInfo };
            process.Start();
            var standardOutput = await process.StandardOutput.ReadToEndAsync();
            var standardError = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            return new CommandResult(process.ExitCode, standardOutput, standardError);
        }
        catch (Exception exception) when (!throwOnFailureToStart && exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            return new CommandResult(1, string.Empty, exception.Message);
        }
    }
}

sealed record CommandResult(int ExitCode, string StandardOutput, string StandardError);
