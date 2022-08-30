﻿using Android.Content;
using Android.Runtime;
using Android.Views;
using Text_search_and_returning;
using Text_search_and_returning.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(EditorExtended), typeof(EditorRendererExtended))]
namespace Text_search_and_returning.Droid
{
    class EditorRendererExtended : EditorRenderer
    {
        public EditorRendererExtended(Context context) : base(context)
        {
        }
        protected override void OnVisibilityChanged(Android.Views.View changedView, [GeneratedEnum] ViewStates visibility)
        {
            base.OnVisibilityChanged(changedView, visibility);
            if (Control != null)
            {
                if (Connections.searcher_text_transmission != null && Connections.start_search_index != -1)
                {
                    if (Control.Text.IndexOf(Connections.searcher_text_transmission, Connections.start_search_index) != -1)
                    {
                        Control.RequestFocus();
                        Control.SetSelection(Control.Text.IndexOf(Connections.searcher_text_transmission, Connections.start_search_index));
                    }
                }
            }
        }
    }
}