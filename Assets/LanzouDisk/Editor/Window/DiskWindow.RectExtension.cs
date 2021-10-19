/*********************************************************************************
 *Author:         OnClick
 *Version:        0.0.1
 *UnityVersion:   2017.2.3p3
 *Date:           2019-05-24
 *Description:    IFramework
 *History:        2018.11--
*********************************************************************************/
using UnityEngine;
namespace LanZouWindow
{
    public enum AnchorType
    {
        UpperLeft = 0,
        UpperCenter = 1,
        UpperRight = 2,
        MiddleLeft = 3,
        MiddleCenter = 4,
        MiddleRight = 5,
        LowerLeft = 6,
        LowerCenter = 7,
        LowerRight = 8
    }

    public static class RectExtension
    {
        public static Rect Zoom(this Rect rect, AnchorType type, float pixel)
        {
            return Zoom(rect,type,new Vector2(pixel, pixel));
        }
        public static Rect Zoom(this Rect rect, AnchorType type,Vector2 pixelOffset)
        {
            float tempW = rect.width + pixelOffset.x;
            float tempH = rect.height + pixelOffset.y;
            switch (type)
            {
                case AnchorType.UpperLeft:
                    break;
                case AnchorType.UpperCenter:
                    rect.x -= (tempW - rect.width) / 2;
                    break;
                case AnchorType.UpperRight:
                    rect.x -= tempW - rect.width;
                    break;
                case AnchorType.MiddleLeft:
                    rect.y -= (tempH - rect.height) / 2;
                    break;
                case AnchorType.MiddleCenter:
                    rect.x -= (tempW - rect.width) / 2;
                    rect.y -= (tempH - rect.height) / 2;
                    break;
                case AnchorType.MiddleRight:
                    rect.y -= (tempH - rect.height) / 2;
                    rect.x -= tempW - rect.width;
                    break;
                case AnchorType.LowerLeft:
                    rect.y -= tempH - rect.height;
                    break;
                case AnchorType.LowerCenter:
                    rect.y -= tempH - rect.height;
                    rect.x -= (tempW - rect.width) / 2;
                    break;
                case AnchorType.LowerRight:
                    rect.y -= tempH - rect.height;
                    rect.x -= tempW - rect.width;
                    break;
            }
            rect.width = tempW;
            rect.height = tempH;
            return rect;
        }

        public static Rect CutBottom(this Rect r, float pixels)
        {
            r.yMax -= pixels;
            return r;
        }
        public static Rect CutTop(this Rect r, float pixels)
        {
            r.yMin += pixels;
            return r;
        }
        public static Rect CutRight(this Rect r, float pixels)
        {
            r.xMax -= pixels;
            return r;
        }
        public static Rect CutLeft(this Rect r, float pixels)
        {
            r.xMin += pixels;
            return r;
        }

        public static Rect Cut(this Rect r, float pixels)
        {
            return r.Margin(-pixels);
        }

        public static Rect Margin(this Rect r, float pixels)
        {
            r.xMax += pixels;
            r.xMin -= pixels;
            r.yMax += pixels;
            r.yMin -= pixels;
            return r;
        }

        public static Rect[] VerticalSplit(this Rect r, float width, float padding = 0, bool justMid = true)
        {
            if (justMid)
                return new Rect[2]{
                r.CutRight((int)(r.width-width)).CutRight(padding).CutRight(-Mathf.CeilToInt(padding/2f)),
                r.CutLeft(width).CutLeft(padding).CutLeft(-Mathf.FloorToInt(padding/2f))
            };
            return new Rect[2]{
                r.CutRight((int)(r.width-width)).Cut(padding).CutRight(-Mathf.CeilToInt(padding/2f)),
                r.CutLeft(width).Cut(padding).CutLeft(-Mathf.FloorToInt(padding/2f))
            };
        }
        public static Rect[] HorizontalSplit(this Rect r, float height, float padding = 0, bool justMid = true)
        {
            if (justMid)
                return new Rect[2]{
                r.CutBottom((int)(r.height-height)).CutBottom(padding).CutBottom(-Mathf.CeilToInt(padding/2f)),
                r.CutTop(height).CutTop(padding).CutTop(-Mathf.FloorToInt(padding/2f))
                };
            return new Rect[2]{
                r.CutBottom((int)(r.height-height)).Cut(padding).CutBottom(-Mathf.CeilToInt(padding/2f)),
                r.CutTop(height).Cut(padding).CutTop(-Mathf.FloorToInt(padding/2f))
            };
        }

    }
}
