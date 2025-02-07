using GroupOrder.Data;
using Microsoft.AspNetCore.Components;

namespace GroupOrder.Components.MyOrder;

public partial class DeleteOrder
{
    [Parameter]
    public Order? Order { get; set; }

    /**
     * This component is purely for user view and therefore allows
     * no admin override
     */
    private bool CanEdit()
    {
        if (gs.Group == null)
            return false;
        if (Order == null)
            return false;
        switch (gs.Group.EditingRule)
        {
            case EditingRule.AllowBeforeDeadline:
                return !IsOrderingClosed();
            case EditingRule.AllowBeforeCartAndDeadline:
                return !IsOrderingClosed() && Order.AddedToCart == false;
            case EditingRule.AllowBeforeCartAndPaymentAndDeadline:
                return !IsOrderingClosed()
                    && Order.AddedToCart == false
                    && Order.Person.GetPriceToPay() - Order.Price >= 0;
            case EditingRule.AskEverytime:
                return false; //TODO
            case EditingRule.NeverAllow:
                return false;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private bool IsOrderingClosed()
    {
        return DateTime.Now > gs.Group!.ClosingTime;
    }

    private void Delete()
    {
        if (CanEdit() && Order != null)
        {
            gs.ReloadRestriction.WaitOne();
            gs.DeleteOrder(Order);
            gs.Save();
            gs.ReloadRestriction.Release();
        }
    }
}
