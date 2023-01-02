using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class NGUISetRenderQueue : MonoBehaviour
{
	public Action OnRenderQueueChanged = null;

	public enum TypeEnum
	{
		ManualSet,
		AutoSet
	}

	public enum DirectionEnum
	{
		Max,
		Min
	}

	public enum MaterialUsedType
	{
		/// <summary>
		/// shared material
		/// 使用共享材质
		/// </summary>
		Shared = 0,
		/// <summary>
		/// 属于同一个父Panel的原本共用一个shared material的render共用一个材质，尚未实现
		/// </summary>
		SpecificToPanel = 1,
		/// <summary>
		/// 每个实例使用的material完全独立
		/// </summary>
		Private = 2,
	}

	[SerializeField]
	public TypeEnum mType = TypeEnum.AutoSet;

	[SerializeField]
	public bool mIncludeChildren = true;

	[SerializeField]
	public int mRenderQueue = 3000;

	[SerializeField]
	public UIPanel mRelativePanel;

	[SerializeField]
	public DirectionEnum mDirection = DirectionEnum.Max;

	//这个东东目前貌似没什么卵用，如果offset为负数，将自动把render queue弄到min之前
	[SerializeField]
	public int mOffset = 1;

	[SerializeField]
	public bool mUsedSharedMaterial = true;

	//private int             lastRelatedPanelRenderQueue;
	public bool needRefresh = false;

	public bool needChangeSorting = false;

	public int mLastRenderQueue = 3000;

	public int mVirtualDrawCallCount = 0;

	[SerializeField]
	public int mSortingOrder = 10;

	[SerializeField]
	public string mSortingLayer = "Default";

	public TypeEnum Type {
		get {
			return mType;
		}
		set {
			mType = value;
			needRefresh = true;
		}
	}

	public bool IncludeChildren {
		get {
			return mIncludeChildren;
		}
		set {
			mIncludeChildren = value;
		}
	}

	public int RenderQueue {
		get {
			return mRenderQueue;
		}
		set {
			mRenderQueue = value;
			needRefresh = true;
		}
	}

	public UIPanel RelativePanel {
		get {
			return mRelativePanel;
		}
		set {
			mRelativePanel = value;
			needRefresh = true;
			RelativePanelCheck ();
		}
	}

	public DirectionEnum Direction {
		get {
			return mDirection;
		}
		set {
			mDirection = value;
			needRefresh = true;
		}
	}

	/// <summary>
	/// 当方向为Max时候，都懂得
	/// 当方向为Min时，offset越大在越下层，大概
	/// </summary>
	public int Offset {
		get {
			return mOffset;
		}
		set {
			mOffset = value;
			needRefresh = true;
		}
	}

	public bool UsedSharedMaterial {
		get {
			return mUsedSharedMaterial;
		}
		set {
			if (value == true) {
				return;
			}
			mUsedSharedMaterial = value;
			needRefresh = true;
		}
	}

	public int SortingOrder {
		get {
			return mSortingOrder;
		}
		set {
			mSortingOrder = value;
			needChangeSorting = true;
		}
	}

	public string SortingLayer {
		get {
			return mSortingLayer;
		}
		set {
			mSortingLayer = value;
			needChangeSorting = true;
		}
	}

	/// <summary>
	/// 上一次SetRenderQueue被调用时，传进来的RenderQueue的值
	/// </summary>
	public int LastRenderQueue {
		get {
			return mLastRenderQueue;
		}
	}

	/// <summary>
	/// 虚拟DrawCall，当设置的值大于0时，实际上将不会改变任何Render，而是直接占用对应个数的DrawCall，
	/// 这些被占用的DrawCall对应地会占用一些RenderQueue，这些RenderQueue可以被随意使用
	/// </summary>
	public int VirtualDrawCallCount {
		get {
			return mVirtualDrawCallCount;
		}
		set {
			mVirtualDrawCallCount = value;
		}
	}

	// Use this for initialization
	public void Start ()
	{
		RelativePanelCheck ();
		ApplySorting ();
		RefreshRenderQueue ();
		//ProtectedListCheck();
	}

	public void LateUpdateCheck ()
	{
		if (mRelativePanel != null && needRefresh) {
			this.RefreshRenderQueue ();
			needRefresh = false;
		}
		if (needChangeSorting) {
			ApplySorting ();
			needChangeSorting = false;
		}
	}

	public void LateUpdate ()
	{
		LateUpdateCheck ();
	}

	public void OnEnable ()
	{
		//RefreshRenderQueue();
	}

	public/// <summary>
	/// 检查相关panel是否存在
	/// </summary>
	void RelativePanelCheck ()
	{
		if (mType == TypeEnum.AutoSet) {
			if (mRelativePanel == null) {
				mRelativePanel = this.GetComponentInParent<UIPanel> ();
				if (mRelativePanel == null) {
					//Debug.Log("No relative panel set. NGUISetRenderQueue - " + this.gameObject.name);
					return;
				}
			}
		}
	}

	public/// <summary>
	/// 这个方法基本只有Manual Set时候会起效果
	/// </summary>
	void ApplySorting ()
	{
		Renderer[] renderers = null;
		if (mIncludeChildren) {
			renderers = this.GetComponentsInChildren<Renderer> (true);
		}
		else {
			renderers = this.GetComponents<Renderer> ();
		}
		if (mRelativePanel == null) {
			if (renderers != null) {
				for (int i = 0; i < renderers.Length; i++) {
					renderers [i].sortingOrder = mSortingOrder;
					renderers [i].sortingLayerName = mSortingLayer;
				}
			}
		}
		else {
			if (renderers != null) {
				for (int i = 0; i < renderers.Length; i++) {
					renderers [i].sortingOrder = mRelativePanel.sortingOrder;
					renderers [i].sortingLayerName = mRelativePanel.sortingLayerName;
				}
			}
		}
	}

	public void RefreshRenderQueue ()
	{
		if (mRelativePanel == null) {
			return;
		}
		UIPanel.RemoveAdditionalRenderQueue (mRelativePanel, SetRenderQueue);
		if (mType == TypeEnum.AutoSet) {
			if (mDirection == DirectionEnum.Max) {
				//mRenderQueue = mRelativePanel.startingRenderQueue + dcs.Count + mOffset;
				UIPanel.AddAdditionalRenderQueue (mRelativePanel, mOffset, SetRenderQueue);
			}
			else {
				//mRenderQueue = mRelativePanel.startingRenderQueue - mOffset;
				UIPanel.AddAdditionalRenderQueue (mRelativePanel, -mOffset, SetRenderQueue);
			}
		}
		else {
			SetRenderQueue (mRenderQueue, mSortingOrder);
		}
		//ProtectedListCheck();
	}

	public int SetRenderQueue (int renderQueue, int sortOrder)
	{
		Renderer[] renderers = null;
		int dcCount = 0;
		if (this == null) {
			return 0;
		}
		bool isRenderQueueSame = mLastRenderQueue == renderQueue;
		mLastRenderQueue = renderQueue;
		if (mVirtualDrawCallCount > 0) {
			dcCount = mVirtualDrawCallCount;
		}
		else {
			if (mIncludeChildren) {
				renderers = this.GetComponentsInChildren<Renderer> (true);
			}
			else {
				renderers = this.GetComponents<Renderer> ();
			}
			if (renderers != null) {
				if (mUsedSharedMaterial) {
					foreach (Renderer renderer in renderers) {
						renderer.sortingOrder = sortOrder;
						if (renderer.sharedMaterial != null) {
							renderer.sharedMaterial.renderQueue = renderQueue;
							dcCount++;
						}
					}
				}
				else {
					foreach (Renderer renderer in renderers) {
						renderer.sortingOrder = sortOrder;
						if (renderer.material != null) {
							renderer.material.renderQueue = renderQueue;
							dcCount++;
						}
					}
				}
			}
		}
		if (!isRenderQueueSame && OnRenderQueueChanged != null) {
			OnRenderQueueChanged ();
		}
		return dcCount;
	}

	public/// <summary>
	/// 为了解决git各种.mat老是被修改而临时写的东西
	/// </summary>
	void OnDestroy ()
	{
		if (mUsedSharedMaterial) {
			if ((mType == TypeEnum.AutoSet)) {
				SetRenderQueue (9999, mSortingOrder);
			}
		}
		else {
			Renderer[] renderers = null;
			if (mIncludeChildren) {
				renderers = this.GetComponentsInChildren<Renderer> (true);
			}
			else {
				renderers = this.GetComponents<Renderer> ();
			}
			foreach (Renderer renderer in renderers) {
				if (renderer.material != null) {
					DestroyImmediate (renderer.material);
				}
			}
			mUsedSharedMaterial = true;
			if ((mType == TypeEnum.AutoSet)) {
				SetRenderQueue (9999, mSortingOrder);
			}
		}
	}

	[ContextMenu ("Show In Top")]
	public void ShowInTop ()
	{
		if (mUsedSharedMaterial) {
			SetRenderQueue (9999, mSortingOrder);
		}
		else {
			mUsedSharedMaterial = true;
			SetRenderQueue (9999, mSortingOrder);
			mUsedSharedMaterial = false;
		}
	}

	[ContextMenu ("Excute")]
	public void Reset ()
	{
		SetRenderQueue (mRenderQueue, mSortingOrder);
	}
}
