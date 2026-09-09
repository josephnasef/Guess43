namespace Guess43.Application.Abstractions;

/// <summary>Hashes and verifies passwords using a proven framework implementation.</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    PasswordVerificationOutcome Verify(string passwordHash, string providedPassword);
}

/// <summary>Result of verifying a password, including whether the hash should be upgraded.</summary>
public enum PasswordVerificationOutcome
{
    Failed = 0,
    Success = 1,
    SuccessRehashNeeded = 2,
}
