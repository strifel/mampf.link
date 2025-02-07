namespace GroupOrder.Components.Layout;

public partial class NavMenu
{
    private string HomeLink()
    {
        return _prefix + _suffix;
    }

    private string OverviewLink()
    {
        return _prefix + "/overview" + _suffix;
    }

    private string AddLink()
    {
        return _prefix + "/add" + _suffix;
    }

    private string FinanceLink()
    {
        return _prefix + "/finance" + _suffix;
    }

    private string CartLink()
    {
        return _prefix + "/cart" + _suffix;
    }
}