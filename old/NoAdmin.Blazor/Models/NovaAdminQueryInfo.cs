using System;
using System.Threading.Tasks;

namespace NoAdmin.Blazor.Models;

public class NovaAdminQueryInfo
{
	private long _total;

	private int _pageNumber = 1;

	public string SearchText { get; set; }

	public string Sort { get; set; }

	public NovaAdminFilterInfo[] Filters { get; set; }

	public long Total
	{
		get
		{
			return _total;
		}
		set
		{
			if (value < 0)
			{
				value = 0L;
			}
			if (value != _total)
			{
				_total = value;
				MaxPageNumber = (int)Math.Ceiling(1.0 * (double)_total / (double)Math.Max(1, PageSize));
				if (_pageNumber > MaxPageNumber)
				{
					_pageNumber = MaxPageNumber;
				}
				if (_pageNumber <= 0)
				{
					_pageNumber = 1;
				}
			}
		}
	}

	public int PageNumber
	{
		get
		{
			return _pageNumber;
		}
		set
		{
			if (value <= 0)
			{
				value = 1;
			}
			_pageNumber = value;
		}
	}

	public int PageSize { get; set; } = 30;

	public int MaxPageNumber { get; private set; }

	public string PageNumberQueryStringName { get; set; } = "page";

	public string SearchTextQueryStringName { get; set; } = "search";

	public string SortQueryStringName { get; set; } = "sort";

	public bool IsQueryString { get; set; } = true;

	public bool IsTracking { get; set; } = true;

	public Func<Task> InvokeQueryAsync { get; set; }

	public Func<Task> InvokeAddAsync { get; set; }
}
