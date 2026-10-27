using System;

/// <summary>
/// 旧接口名称的兼容别名。
/// 新代码应使用 IKitchenObjectHolder。
/// </summary>
[Obsolete("Use IKitchenObjectHolder instead.")]
public interface Ikitchenobjectparent : IKitchenObjectHolder
{
}
