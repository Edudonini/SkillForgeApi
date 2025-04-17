using System.Text.RegularExpressions;

namespace SkillForge.Domain.ValueObjects;

/// <summary>Value‑Object imutável que representa um e‑mail válido.</summary>
public sealed record Email
{
    private static readonly Regex _regex =
        new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);

    public string Address { get; }

    private Email(string address) => Address = address;

    public static Email Create(string address)
    {
        if (string.IsNullOrWhiteSpace(address) || !_regex.IsMatch(address))
            throw new ArgumentException("Endereço de e‑mail inválido.", nameof(address));

        return new Email(address.Trim().ToLowerInvariant());
    }

    public override string ToString() => Address;
}
