namespace GroupOrder.Components.Group;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

// ReSharper disable once InconsistentNaming
public partial class PaymentQR
{
    [Parameter]
    public string? Content { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await JsRuntime.InvokeAsync<object>("setQRContent", Content);
    }
}
