
namespace TalkerMakerDeluxe
{
    // Common interface for items that can be selected
    // on the DesignerCanvas; used by DesignerItem and Connection
    public interface ISelectable
    {
        bool IsSelected { get; set; }
    }
}
