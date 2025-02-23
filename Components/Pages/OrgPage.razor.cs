using System.Security.Cryptography;
using GroupOrder.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace GroupOrder.Components.Pages;

public partial class OrgPage
{
    [Parameter]
    public string? VanitySlug { get; set; }

    private GroupContext? _context;
    private bool _loading;
    private VanityURL? _vanityUrl;

    protected override async Task OnParametersSetAsync()
    {
        if (_loading)
        {
            return; //avoid concurrent requests.
        }

        _loading = true;

        _context ??= await DbFactory.CreateDbContextAsync();

        _vanityUrl = await _context
            .VanityUrls.Include(v => v.History)
            .SingleOrDefaultAsync(c => c.Slug == VanitySlug);

        _loading = false;
    }

    private void GroupCreateCallback(Data.Group group)
    {
        if (_vanityUrl == null)
            return;
        if (_context == null)
            return;

        // Generate a new unique slug for the group that previously used the vanity URL
        if (_vanityUrl.History.Count > 0)
        {
            _vanityUrl.History.Last().GroupSlug = RandomNumberGenerator.GetHexString(10, true);
        }

        // Set slug of new group to vanity URL
        group.GroupSlug = _vanityUrl.Slug;
        _vanityUrl.History.Add(group);
        _vanityUrl.UsedAt = DateTime.Now;
        _context.Add(group);
        _context.SaveChanges();

        NavigationManager.NavigateTo($"/group/{group.GroupSlug}/overview?admin={group.AdminCode}");
    }

    private int GetExistingTimeOfCurrentGroup()
    {
        return (int)(DateTime.Now - _vanityUrl!.UsedAt).TotalMinutes;
    }
}
