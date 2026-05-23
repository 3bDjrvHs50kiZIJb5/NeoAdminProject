namespace NoAdmin.Blazor.Models;

public enum NovaModalAnimation
{
	/// <summary>
	/// 从顶部滑入，弹窗从屏幕顶部平滑地向下滑动出现
	/// </summary>
	Default,
	/// <summary>
	/// 从底部滑入，弹窗从屏幕底部平滑地向上滑动出现
	/// </summary>
	SlideUp,
	/// <summary>
	/// 缩放出现，弹窗从中心一个很小的点放大出现
	/// </summary>
	Zoom,
	/// <summary>
	/// 报纸效果，弹窗旋转并放大出现，像一份展开的报纸
	/// </summary>
	Newspaper,
	/// <summary>
	/// 3D 垂直翻转，弹窗像一张卡片一样，沿着水平轴进行 3D 翻转
	/// </summary>
	Flip,
	/// <summary>
	/// 优雅坠落，弹窗从上方轻微旋转并坠落到屏幕中央
	/// </summary>
	Fall
}
