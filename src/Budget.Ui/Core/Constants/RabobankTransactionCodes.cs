namespace Budget.Ui.Core.Constants;

/// <summary>
/// Bevat de constanten voor Rabobank transactiesoortcodes (mutatiecodes).
/// </summary>
public static class RabobankTransactionCodes
{
    /// <summary>
    /// Een reguliere pinbetaling in een winkel (pas in de automaat gestoken).
    /// </summary>
    public const string Betaalautomaat = "ba";

    /// <summary>
    /// Een pinbetaling waarbij je contactloos hebt betaald (pas of mobiel tegen de automaat).
    /// </summary>
    public const string BetalenContactloos = "bc";

    /// <summary>
    /// Een standaard overschrijving (van of naar een andere rekening). Dit zijn de "gewone" overboekingen.
    /// </summary>
    public const string Bankgiro = "bg";

    /// <summary>
    /// Een uitgaande betaling aan een bedrijf/crediteur, vaak via een zakelijk contract of batchbetaling.
    /// </summary>
    public const string Crediteurenbetaling = "cb";

    /// <summary>
    /// Vaak gebruikt voor bankkosten, rentebijschrijvingen of -afschrijvingen en kosten voor de betaalpas.
    /// </summary>
    public const string Diversen = "db";

    /// <summary>
    /// Historisch (Eurocheque). Tegenwoordig soms gebruikt voor specifieke (zakelijke) incasso's of correcties.
    /// </summary>
    public const string Eurocheque = "ec";

    /// <summary>
    /// Een automatische incasso (SEPA Direct Debit). Bijvoorbeeld voor je abonnementen, huur of energierekening.
    /// </summary>
    public const string EuroIncasso = "ei";

    /// <summary>
    /// Contant geld opnemen bij een pinautomaat (ATM).
    /// </summary>
    public const string Geldautomaat = "ga";

    /// <summary>
    /// Een betaling die je online hebt gedaan via iDEAL (bijv. bij een webshop).
    /// </summary>
    public const string Ideal = "id";

    /// <summary>
    /// Een bijschrijving die door de bank wordt herkend als salaris.
    /// </summary>
    public const string Salarisbetaling = "sb";

    /// <summary>
    /// Een overboeking van of naar je eigen Rabobank (spaar)rekening.
    /// </summary>
    public const string EigenRekening = "tb";
}