//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Controls;

namespace LevelEditorCore
{
    /// <summary>
    /// A thumbnail viewer control</summary>
    internal class ThumbnailControl : Panel
    {
        
        /// <summary>
        /// Constructor</summary>
        public ThumbnailControl()
        {
            // Virtual member call in constructor...
            m_fontHeight = Font.Height;

            m_hoverTimer = new Timer { Interval = 500 };
            m_hoverTimer.Tick += HoverTimerTick;

            base.DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);

            m_items = new List<ThumbnailControlItem>();
        }

        /// <summary>
        /// Constructor with thumbnail orientation and dimension</summary>
        /// <param name="orientation">Orientation of thumbnail images</param>
        /// <param name="maxDimension">Maximum dimension of thumbnail images</param>
        public ThumbnailControl(Orientation orientation, int maxDimension)
            : this()
        {
            m_orientation = orientation;
            m_thumbnailSize = maxDimension;
        }

        /// <summary>
        /// Constructor with thumbnail orientation</summary>
        /// <param name="orientation">Orientation of thumbnail images</param>
        public ThumbnailControl(Orientation orientation)
            : this()
        {
            m_orientation = orientation;
        }

        /// <summary>
        /// Gets and sets the orientation of thumbnail images</summary>
        public Orientation Orientation
        {
            get { return m_orientation; }
            set
            {
                if (m_orientation == value)
                    return;

                m_orientation = value;
                RecalculateClientSize();
            }
        }

        /// <summary>
        /// Gets and sets the maximum dimension of a thumbnail in the viewer</summary>
        public int ThumbnailSize
        {
            get { return m_thumbnailSize; }
            set
            {
                if (m_thumbnailSize != value)
                {
                    m_thumbnailSize = value;
                    RecalculateClientSize();
                }
            }
        }

        /// <summary>
        /// Gets the thumbnail items</summary>
        public IList<ThumbnailControlItem> Items
        {
            get { return m_items; }
        }

        /// <summary>
        /// Gets the selection</summary>
        public Selection<ThumbnailControlItem> Selection
        {
            get { return m_selection; }
        }

        /// <summary>
        /// Picks the thumbnail beneath the given viewer point</summary>
        /// <param name="point">Point, in client coordinates</param>
        /// <returns>Thumbnail beneath the given client point, or null</returns>
        public ThumbnailControlItem PickThumbnail(Point point)
        {
            Point position = new Point(
                ThumbnailMargin + AutoScrollPosition.X,
                ThumbnailMargin + AutoScrollPosition.Y);

            foreach (ThumbnailControlItem item in m_items)
            {
                Rectangle thumbRect = GetThumbnailBoundaryRect(position);
                if (thumbRect.Contains(point))
                    return item;

                position = NextThumbnailPosition(position);
            }

            return null;
        }

        /// <summary>
        /// Gets and sets the indicator image list</summary>
        public ImageList IndicatorImageList
        {
            get { return m_indicatorImages; }
            set
            {
                m_indicatorImages = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Raises the FontChanged event</summary>
        /// <param name="e">Event args</param>
        protected override void OnFontChanged(EventArgs e)
        {
            m_fontHeight = Font.Height;
            base.OnFontChanged(e);
        }

        /// <summary>
        /// Raises the Paint event</summary>
        /// <param name="e">Event args</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            Point position = new Point(
                ThumbnailMargin + AutoScrollPosition.X,
                ThumbnailMargin + AutoScrollPosition.Y);


            Pen selPen = new Pen(Color.LimeGreen, 2.0f);
            StringFormat format = new StringFormat();
            var txtBrush = new SolidBrush(ForeColor);
            try                
            {
                format.Alignment = StringAlignment.Center;

                HashSet<ThumbnailControlItem> selected = new HashSet<ThumbnailControlItem>(m_selection);

                foreach (ThumbnailControlItem item in m_items)
                {
                    Image image = item.Image;
                    Rectangle thumbRect = GetThumbnailRect(position, image);

                    e.Graphics.DrawImage(image, thumbRect);

                    RectangleF captionRect = new RectangleF(
                        position.X,
                        position.Y + m_thumbnailSize + ThumbnailMargin,
                        m_thumbnailSize,
                        m_fontHeight);

                    e.Graphics.DrawString(
                        item.Name,
                        Font,
                        txtBrush,
                        captionRect,
                        format);

                    if (m_indicatorImages != null &&
                        item.Indicator != null)
                    {
                        Image indicator = m_indicatorImages.Images[item.Indicator];
                        if (indicator != null)
                        {
                            Rectangle indicatorRect = new Rectangle(
                                new Point(position.X - 2, position.Y - 2),
                                m_indicatorImages.ImageSize);

                            e.Graphics.DrawImage(indicator, indicatorRect);
                        }
                    }

                    Rectangle boundRect = GetThumbnailBoundaryRect(position);
                    e.Graphics.DrawRectangle(
                        selected.Contains(item) ? selPen : Pens.Black, boundRect);

                    position = NextThumbnailPosition(position);
                }
            }
            finally
            {
                txtBrush.Dispose();
                selPen.Dispose();
                format.Dispose();
            }

            if (m_multiSelecting)
            {
                e.Graphics.DrawRectangle(Pens.DarkGray, GetSelectionRect());
            }
        }

        /// <summary>
        /// Raises the SizeChanged event</summary>
        /// <param name="e">Event args</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            RecalculateClientSize();
            base.OnSizeChanged(e);
        }

        /// <summary>
        /// Raises the MouseDown event</summary>
        /// <param name="e">Event args</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.Focus();
            if (e.Button == MouseButtons.Left)
            {
                ThumbnailControlItem hitItem = PickThumbnail(e.Location);
                if (hitItem != null)
                {
                    if (!m_selection.Contains(hitItem))
                    {
                        ThumbnailControlItem[] items = { hitItem };
                        SelectItem(items);
                    }
                }
                else
                {
                    m_multiSelecting = true;
                    m_startPt = e.Location;
                    m_mousePt = e.Location;
                }
            }
            this.Invalidate();
            base.OnMouseDown(e);
        }

        /// <summary>
        /// Raises the MouseMove event</summary>
        /// <param name="e">Event args</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.None)
            {
                ThumbnailControlItem thumbnail = PickThumbnail(new Point(e.X, e.Y));
                if (thumbnail != m_hoverThumbnail)
                {
                    m_hoverThumbnail = thumbnail;
                    m_hoverTimer.Start();
                }
            }

            if (m_multiSelecting)
            {
                m_mousePt = e.Location;
                Invalidate();
            }

            base.OnMouseMove(e);
        }

        /// <summary>
        /// Raises the MouseUp event</summary>
        /// <param name="e">Event args</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {                               
                if (m_multiSelecting)
                {
                    m_multiSelecting = false;

                    IEnumerable<ThumbnailControlItem> picked = Pick(GetSelectionRect());
                    SelectItem(picked);                    
                }                                
            }

            base.OnMouseUp(e);
            Invalidate();
            EndHover();
        }

        private void SelectItem(IEnumerable<ThumbnailControlItem> items)
        {     
            
            bool extend = ModifierKeys == Keys.Shift;
            bool toggle = ModifierKeys == Keys.Control;

            if(toggle)
            {
                m_selection.BeginUpdate();
                foreach(var item in items)
                {
                    if(m_selection.Contains(item))
                        m_selection.Remove(item);
                    else
                        m_selection.Add(item);

                }  
                m_selection.EndUpdate();
            }
            else if(extend)
            {
                m_selection.AddRange(items);                    
            }
            else
            {
                m_selection.SetRange(items);
            }            
        }

      

        /// <summary>
        /// Raises the MouseLeave event</summary>
        /// <param name="e">Event args</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            EndHover();
            base.OnMouseLeave(e);
        }

        private void HoverTimerTick(object sender, EventArgs e)
        {
            EndHover();

            if (m_hoverLabel == null && m_hoverThumbnail != null)
            {
                m_hoverLabel =
                    new HoverLabel(m_hoverThumbnail.Description)
                    {
                        Location = new Point(MousePosition.X - 8, MousePosition.Y + 8)
                    };
                m_hoverLabel.ShowWithoutFocus();
            }
        }

        private void EndHover()
        {
            if (m_hoverLabel != null)
            {
                m_hoverLabel.Hide();
                m_hoverLabel.Dispose();
                m_hoverLabel = null;
                m_hoverThumbnail = null;
            }
            m_hoverTimer.Stop();
        }

        public void RecalculateClientSize()
        {
            // As of February 4, 2011
            // Orientation.Horizontal
            //  means that scrolling is horizontal (if any)
            //  items are bounded vertically
            //  items are ordered top to bottom before going to the next column
            // Orientation.Vertical
            //  means that scrolling is vertical (if any)
            //  items are bounded horizontally
            //  items are ordered left to right before going to the next row

            Size size = new Size(ThumbnailMargin, ThumbnailMargin);

            if (m_items.Count > 0)
            {
                int thumbsPerCol;
                int thumbsPerRow;

                if (m_orientation == Orientation.Horizontal)
                {
                    thumbsPerCol = (ClientSize.Height - ThumbnailMargin) / (m_thumbnailSize + (2 * ThumbnailMargin) + m_fontHeight);
                    if (thumbsPerCol < 1) thumbsPerCol = 1;

                    thumbsPerRow = (m_items.Count / thumbsPerCol) + 1;
                    if ((m_items.Count % 2) == 0)
                        thumbsPerRow -= 1;

                    if (thumbsPerRow < 1) thumbsPerRow = 1;
                }
                else // m_orientation == Orientation.Vertical
                {
                    thumbsPerRow = (ClientSize.Width - ThumbnailMargin) / (m_thumbnailSize + ThumbnailMargin);
                    if (thumbsPerRow < 1) thumbsPerRow = 1;

                    thumbsPerCol = (m_items.Count / thumbsPerRow) + 1;
                    if (((thumbsPerRow * thumbsPerCol) - m_items.Count) >= thumbsPerRow)
                        thumbsPerCol -= 1;

                    if (thumbsPerCol < 1) thumbsPerCol = 1;
                }

                size.Width = ((thumbsPerRow) * (m_thumbnailSize + ThumbnailMargin)) + ThumbnailMargin;
                size.Height = ((thumbsPerCol) * (m_thumbnailSize + (2 * ThumbnailMargin) + m_fontHeight)) + ThumbnailMargin;
                m_clientSize = size;
            }

            AutoScrollMinSize = size;
            Invalidate();
        }

        private Rectangle GetThumbnailRect(Point thumbLoc, Image image)
        {
            Size thumbSize = GetThumbnailSize(image);

            int xInset = Math.Max(0, m_thumbnailSize - thumbSize.Width) / 2;
            int yInset = Math.Max(0, m_thumbnailSize - thumbSize.Height) / 2;
            thumbLoc.Y += yInset;
            thumbLoc.X += xInset;

            return new Rectangle(thumbLoc, thumbSize);
        }

        private Rectangle GetThumbnailBoundaryRect(Point position)
        {
            return new Rectangle(position.X - 4, position.Y - 4,
                m_thumbnailSize + 8, m_thumbnailSize + 8);
        }

        private Rectangle GetSelectionRect()
        {
            int x = Math.Min(m_startPt.X, m_mousePt.X);
            int y = Math.Min(m_startPt.Y, m_mousePt.Y);
            int w = Math.Abs(m_mousePt.X - m_startPt.X);
            int h = Math.Abs(m_mousePt.Y - m_startPt.Y);

            return new Rectangle(x, y, w, h);
        }

        private IEnumerable<ThumbnailControlItem> Pick(Rectangle rect)
        {
            List<ThumbnailControlItem> picked = new List<ThumbnailControlItem>();

            Point position = new Point(
                ThumbnailMargin + AutoScrollPosition.X,
                ThumbnailMargin + AutoScrollPosition.Y);

            foreach (ThumbnailControlItem item in m_items)
            {
                Rectangle thumbRect = GetThumbnailBoundaryRect(position);
                if (rect.IntersectsWith(thumbRect))
                    picked.Add(item);

                position = NextThumbnailPosition(position);
            }

            return picked;
        }

        private Size GetThumbnailSize(Image image)
        {
            Size size = image.Size;
            int maxDimension = Math.Max(size.Width, size.Height);
            if (maxDimension > m_thumbnailSize)
            {
                double scale = (double)m_thumbnailSize / (double)maxDimension;
                size.Width = (int)(size.Width * scale);
                size.Height = (int)(size.Height * scale);
            }

            return size;
        }

        private Point NextThumbnailPosition(Point curPosition)
        {            
            // See comment in RecalculateClientSize() about drawing order

            Point position = curPosition;

            if (m_orientation == Orientation.Horizontal)
            {
                position.Y += m_thumbnailSize + m_fontHeight + (2 * ThumbnailMargin);
                if (position.Y >= (m_clientSize.Height + AutoScrollPosition.Y - ThumbnailMargin))
                {
                    position.X += ThumbnailMargin + m_thumbnailSize;
                    position.Y = ThumbnailMargin + AutoScrollPosition.Y;
                }
            }
            else // m_orientation == Orientation.Vertical
            {
                position.X += m_thumbnailSize + ThumbnailMargin;
                if (position.X >= (m_clientSize.Width + AutoScrollPosition.X - ThumbnailMargin))
                {
                    position.X = ThumbnailMargin + AutoScrollPosition.X;
                    position.Y += (m_thumbnailSize + (2 * ThumbnailMargin) + m_fontHeight);
                }
            }
            return position;
        }



        private readonly List<ThumbnailControlItem> m_items;
        private readonly Selection<ThumbnailControlItem> m_selection = new Selection<ThumbnailControlItem>();
        private Orientation m_orientation = Orientation.Vertical;
        private int m_fontHeight;
        private int m_thumbnailSize = 96;
        private readonly Timer m_hoverTimer;
        private ThumbnailControlItem m_hoverThumbnail;
        private HoverLabel m_hoverLabel;
        private const int ThumbnailMargin = 16;
        private Size m_clientSize;
        private ImageList m_indicatorImages;
        private bool m_multiSelecting;
        private Point m_startPt;
        private Point m_mousePt;
    }


    /// <summary>
    /// Item in a ThumbnailControl</summary>
    public class ThumbnailControlItem
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="image">Thumbnail image</param>
        public ThumbnailControlItem(Image image, Uri uri)
        {
            m_image = image;
            m_uri = uri;
            Name = System.IO.Path.GetFileName(m_uri.LocalPath);
            Description = m_uri.LocalPath;
        }

        /// <summary>
        /// Gets and sets the image</summary>
        public Image Image
        {
            get { return m_image; }
            set
            {
                m_image = value;                
            }
        }

        /// <summary>
        /// Gets and sets the indicator</summary>
        public string Indicator
        {
            get { return m_indicator; }
            set
            {
                m_indicator = value;                
            }
        }

        /// <summary>
        /// Gets and sets the tag</summary>
        public Uri Uri
        {
            get { return m_uri; }
          // set { m_uri = value; }
        }

        /// <summary>
        /// Gets and sets the name</summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value;}
        }

        /// <summary>
        /// Gets and sets the description</summary>
        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        private Image m_image;
        private Uri m_uri;
        private string m_indicator;
        private string m_name;
        private string m_description;
    }

}
