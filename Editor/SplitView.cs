using UnityEngine.UIElements;

public class SplitView : TwoPaneSplitView
{
    private new class UxmlFactory : UxmlFactory<SplitView, TwoPaneSplitView.UxmlTraits> { }
}