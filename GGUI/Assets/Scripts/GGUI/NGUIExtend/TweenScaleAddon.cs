using UnityEngine;
using System.Collections;

public class TweenScaleAddon : TweenScale
{
	public int updateTableDepth = 1;

	protected override void OnUpdate(float factor, bool isFinished)
	{
		base.OnUpdate (factor, isFinished);
		if (updateTable) {
			UITable parentTable = NGUITools.FindInParents<UITable> (transform);
			int i = 1;
			while (i < updateTableDepth) {
				parentTable = NGUITools.FindInParents<UITable> (parentTable.transform.parent);
				parentTable.repositionNow = true;
				i++;
			}
		}
	}
}
