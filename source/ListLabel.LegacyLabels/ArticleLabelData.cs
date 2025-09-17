namespace ListLabel.ArticleLabels;

public class ArticleLabelData
{
    
    public string Name1 { get; set; }
    public string Name2 { get; set; }
    public int Expiration { get; set; }
    public int LabelTypeId { get; set; }
    public string ArtNr { get; set; }
    public int UnitWeight { get; set; }
    public decimal LabelPrice { get; set; }
    public string Description { get; set; }
    public string Measure { get; set; }
    public string Index { get; set; }
    public string Origin { get; set; }
    public string UnitAdditionalInfo { get; set; }
    public string? Barcode { get; set; }
    public int? FontSizeName { get; set; }
    public int? FontSizeDescription { get; set; }
    public string? RichTextLabel { get; set; }
}