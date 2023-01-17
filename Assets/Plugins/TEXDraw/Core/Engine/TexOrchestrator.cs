using System;
using UnityEngine;

namespace TexDrawLib
{
    public class TexOrchestrator
    {

        public TexModuleParser parser;
        static TexModuleParser staticParser;
        public TexParserState parserState;
        public TexBoxingState boxingState;
        public TexRendererState rendererState;

        public Atom latestAtomCache;
        public Box latestBoxCache;

        public float signedCoeff = 0;
        public float initialSize = 16;
        public float fitScaling = 1;
        public int initialFont = -1;
        public Color32 initialColor = Color.white;
        public float pixelsPerInch = 96f;
        public Vector2 alignment = Vector2.zero;
        public Overflow overflowMode;

        public Rect canvasRect;
        public Rect clipRect;
        public TexRectOffset clipOffset;

        public Vector2 outputNativeCanvasSize;

        /// <summary>
        /// </summary>
        /// <param name="area"></param>
        /// <param name="scroll"></param>
        /// <param name="offset"></param>
        /// <returns>True if box size changed: Need to recalculate box</returns>
        public bool InputCanvasSize(Rect area, Rect scroll, TexRectOffset offset, Overflow overflow)
        {
            area = offset.Shrink(area);
            clipRect = area;
            clipOffset = offset;

            if (scroll.width > area.width) area.width = scroll.width;
            if (scroll.height > area.height) area.height = scroll.height;

            area.x += scroll.x;
            area.y += scroll.y;

            bool flag = false;
            if (canvasRect.size != area.size || overflowMode != overflow)
                flag = true;
            canvasRect = area;
            overflowMode = overflow;
            return flag;
        }

        public TexOrchestrator()
        {
            if (staticParser == null)
            {
                staticParser = new TexModuleParser();
            }
            parser = staticParser;
            parserState = new TexParserState();
            boxingState = new TexBoxingState();
            rendererState = new TexRendererState();
        }

        public void ResetParser()
        {
            latestAtomCache?.Flush();

            parserState.Reset();
            if (initialFont >= 0)
                parserState.Font.Reset(initialFont);
            parserState.Size.Reset(initialSize);
            parserState.Color.Reset(initialColor);
            parserState.Document.initialSize = initialSize;
            parserState.Paragraph.alignment = alignment.x;
            parserState.Document.pixelsPerInch = pixelsPerInch;
            parserState.parser = parser;
        }

        protected void ResetBoxing(float scale = 1)
        {
            latestBoxCache?.Flush();
            boxingState.Reset(canvasRect.size * scale);
        }

        public void Parse(string input)
        {
            ResetParser();
            latestAtomCache = parser.Parse(input, parserState);
        }

        public void Box()
        {
            fitScaling = 1;
            do
            {
                ResetBoxing(1 / fitScaling);
                fitScaling -= 1 / initialSize;
                var box = latestBoxCache = latestAtomCache.CreateBox(boxingState);
                outputNativeCanvasSize = new Vector2(box.width + clipOffset.horizontal, box.TotalHeight + clipOffset.vertical);
            } while (overflowMode == Overflow.BestFit && (outputNativeCanvasSize.y > canvasRect.height / fitScaling || outputNativeCanvasSize.x > canvasRect.width / fitScaling) && fitScaling > 0);
            fitScaling += 1 / initialSize;
        }

        public void Render()
        {
            var box = latestBoxCache;
            var size = new Vector3(box.width, box.height, box.depth);
            rendererState.Initialize(canvasRect, size, alignment);
            rendererState.clipRect = clipRect;
            rendererState.scale = initialSize;
            rendererState.signedCoeff = signedCoeff;
            rendererState.clip = overflowMode == Overflow.Hidden;
            box.Draw(rendererState);
            if (overflowMode == Overflow.DownScale)
            {
                fitScaling = Mathf.Min(1, Mathf.Min(canvasRect.width / box.width, canvasRect.height / box.TotalHeight));
            }
            if (fitScaling != 1)
            {
                var pivot = new Vector2(1-canvasRect.xMax / canvasRect.width, 1-canvasRect.yMax / canvasRect.height);
                var offset = new Vector3(
                     canvasRect.width * (1 - fitScaling) * -(overflowMode == Overflow.BestFit ? pivot.x : (0.5f-alignment.x)), 
                    (alignment.y - pivot.y) * canvasRect.height * (1 - fitScaling));
                foreach (var vertex in rendererState.vertexes)
                {
                    for (int i = 0; i < vertex.m_Positions.Count; i++)
                    {
                        vertex.m_Positions[i] = vertex.m_Positions[i] * fitScaling + offset;
                    }
                }
            }
        }

        private static string TraceAtom(Atom atom, int depth)
        {
            var s = new string('\t', depth) + atom.ToString() + "\r\n";
            if (atom is LayoutAtom layout)
            {
                foreach (var item in layout.children)
                {
                    s += TraceAtom(item, depth + 1);
                }
            }
            else if (atom is BlockAtom block)
            {
                if (block.atom != null)
                    s += TraceAtom(block.atom, depth + 1);
            }
            else if (atom is InlineAtom inline)
            {
                if (inline.atom != null)
                    s += TraceAtom(inline.atom, depth + 1);
            }
            else if (atom is ScriptsAtom scripts)
            {
                s += TraceAtom(scripts.atom, depth + 1);
                if (scripts.superscript != null)
                    s += TraceAtom(scripts.superscript, depth + 1);
                if (scripts.subscript != null)
                    s += TraceAtom(scripts.subscript, depth + 1);
            }
            else if (atom is FractionAtom fractions)
            {
                if (fractions.numerator != null)
                    s += TraceAtom(fractions.numerator, depth + 1);
                if (fractions.denominator != null)
                    s += TraceAtom(fractions.denominator, depth + 1);
            }
            else if (atom is BoxedAtom box)
            {
                s += TraceAtom(box.atom, depth + 1);
            }
            else if (atom is BoxedLinkAtom link)
            {
                s += TraceAtom(link.atom, depth + 1);
            }
            return s;
        }

        private static string TraceBox(Box box, int depth)
        {
            var s = new String('\t', depth) + box.ToString() + "\r\n";
            if (box is LayoutBox layout)
            {
                foreach (var item in layout.children)
                {
                    s += TraceBox(item, depth + 1);
                }
            }
            else if (box is LinkBox link)
            {
                s += TraceBox(link.box, depth + 1);
            }
            return s;
        }

        public string Trace()
        {
            string s = "";
            if (latestAtomCache != null)
            {
                s += TraceAtom(latestAtomCache, 0);
            }
            s += "\r\n";
            if (latestBoxCache != null)
            {
                s += TraceBox(latestBoxCache, 0);
            }
            return s;
        }
    }
}
