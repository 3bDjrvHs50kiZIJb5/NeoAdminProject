using System.Linq;
using FreeSql;

namespace NoAdmin.Blazor.Models;

public record NovaAdminQueryEventArgs<TItem>(ISelect<TItem> Select, string SearchText, NovaAdminFilterInfo[] Filters, string Sort)
{
	public IQueryable<TItem> Queryable { get; set; }
}
