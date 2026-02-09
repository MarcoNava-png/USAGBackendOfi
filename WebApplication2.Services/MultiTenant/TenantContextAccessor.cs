using WebApplication2.Core.Models.MultiTenant;

namespace WebApplication2.Services.MultiTenant;

public interface ITenantContextAccessor
{
    TenantContext? TenantContext { get; set; }
}

public class TenantContextAccessor : ITenantContextAccessor
{
    private static readonly AsyncLocal<TenantContextHolder> _tenantContextCurrent = new();

    public TenantContext? TenantContext
    {
        get => _tenantContextCurrent.Value?.Context;
        set
        {
            var holder = _tenantContextCurrent.Value;
            if (holder != null)
            {
                holder.Context = null;
            }

            if (value != null)
            {
                _tenantContextCurrent.Value = new TenantContextHolder { Context = value };
            }
        }
    }

    private class TenantContextHolder
    {
        public TenantContext? Context { get; set; }
    }
}
