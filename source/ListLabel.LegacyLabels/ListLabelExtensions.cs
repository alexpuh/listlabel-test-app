using combit.Reporting;
using ListLabel.ArticleLabels;

namespace ListLabel.LegacyLabels;

public static class ListLabelExtensions
{
    public static void DefineLegacyLabelVariables(this combit.Reporting.ListLabel ll, ArticleLabelData data)
    {
        ll.Variables.Add("Haltgr.Frist", data.Expiration);
        ll.Variables.Add("Auftrag.Etiktyp", data.LabelTypeId);
        ll.Variables.Add("Artikel.Bez1", data.Name1);
        ll.Variables.Add("Artikel.Bez2", data.Name2);
        ll.Variables.Add("Artikel.ar_nr", data.ArtNr);
        ll.Variables.Add("Auftrag.Gewicht", data.UnitWeight.ToString());
        ll.Variables.Add("Auftrag.Vk", data.LabelPrice);
        ll.Variables.Add("Adressen.Bez1", String.Empty);
        ll.Variables.Add("Adressen.Bez2", String.Empty);
        ll.Variables.Add("Adressen.Bez3", String.Empty);
        ll.Variables.Add("Artikel.Beschreibung", data.Description);
        ll.Variables.Add("Artikel.me", data.Measure);
        ll.Variables.Add("Artikel.Index", data.Index);
        ll.Variables.Add("Artikel.Herkunft", data.Origin);
        ll.Variables.Add("ArtVk.ZusatzInfo", data.UnitAdditionalInfo);
        ll.Variables.Add("LOC.PreisPro100g", data.LabelPrice/data.UnitWeight);
        
        ll.Variables.Add("ArtVK.Barcode", new LlBarcode(data.Barcode ?? String.Empty, LlBarcodeType.EAN13));
        ll.Variables.Add("FontSize.Name", data.FontSizeName);
        ll.Variables.Add("FontSize.Beschreibung", data.FontSizeDescription);
        
        
        ll.Variables.Add("FreiEtikett", data.RichTextLabel, LlFieldType.RTF);
        ll.Variables.Add("Artikel.Zusatz", data.RichTextLabel, LlFieldType.RTF);
    }

    public static void SetupDesignLegacyLabel(this combit.Reporting.ListLabel ll)
    {
        ll.Design();
    }
}