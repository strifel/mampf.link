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
        if (GroupService.CurrentGroup == null)
            return false;
        if (Order == null)
            return false;
        switch (GroupService.CurrentGroup.EditingRule)
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
        return DateTime.Now > GroupService.CurrentGroup!.ClosingTime;
    }

    private void Delete()
    {
        if (CanEdit() && Order != null)
        {
            GroupService.ReloadRestriction.WaitOne();
            GroupService.DeleteOrder(Order);
            GroupService.Save();
            GroupService.ReloadRestriction.Release();
        }
    }
}
