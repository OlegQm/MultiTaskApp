package crc64ca39f6032cd4cd4f;


public class SearcherRenderExtended
	extends crc643f46942d9dd1fff9.SearchBarRenderer
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onVisibilityChanged:(Landroid/view/View;I)V:GetOnVisibilityChanged_Landroid_view_View_IHandler\n" +
			"";
		mono.android.Runtime.register ("ToothScan.Droid.SearcherRenderExtended, ToothScan.Android", SearcherRenderExtended.class, __md_methods);
	}


	public SearcherRenderExtended (android.content.Context p0)
	{
		super (p0);
		if (getClass () == SearcherRenderExtended.class)
			mono.android.TypeManager.Activate ("ToothScan.Droid.SearcherRenderExtended, ToothScan.Android", "Android.Content.Context, Mono.Android", this, new java.lang.Object[] { p0 });
	}


	public SearcherRenderExtended (android.content.Context p0, android.util.AttributeSet p1)
	{
		super (p0, p1);
		if (getClass () == SearcherRenderExtended.class)
			mono.android.TypeManager.Activate ("ToothScan.Droid.SearcherRenderExtended, ToothScan.Android", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android", this, new java.lang.Object[] { p0, p1 });
	}


	public SearcherRenderExtended (android.content.Context p0, android.util.AttributeSet p1, int p2)
	{
		super (p0, p1, p2);
		if (getClass () == SearcherRenderExtended.class)
			mono.android.TypeManager.Activate ("ToothScan.Droid.SearcherRenderExtended, ToothScan.Android", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android:System.Int32, mscorlib", this, new java.lang.Object[] { p0, p1, p2 });
	}


	public void onVisibilityChanged (android.view.View p0, int p1)
	{
		n_onVisibilityChanged (p0, p1);
	}

	private native void n_onVisibilityChanged (android.view.View p0, int p1);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
