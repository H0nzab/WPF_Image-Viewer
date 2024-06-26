﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

public class CroppingTool
{
    public Rectangle croppingRectangle;
    public Thumb topLeft, topRight, bottomLeft, bottomRight;
    private Canvas OverlayCanvas;
    private Point _startPoint;
    private bool _isDragging;

    public CroppingTool(Canvas overlayCanvas)
    {
        OverlayCanvas = overlayCanvas;
        InitializeCroppingTools();
    }

    private void InitializeCroppingTools()
    {
        CreateCroppingRectangle();
        AddThumbs();
    }

    public void HideCroppingTools()
    {
        croppingRectangle.Visibility = Visibility.Collapsed;

        topLeft.Visibility = Visibility.Collapsed;
        topRight.Visibility = Visibility.Collapsed;
        bottomLeft.Visibility = Visibility.Collapsed;
        bottomRight.Visibility = Visibility.Collapsed;
    }

    public void ShowCroppingTools()
    {
        croppingRectangle.Visibility = Visibility.Visible;

        topLeft.Visibility = Visibility.Visible;
        topRight.Visibility = Visibility.Visible;
        bottomLeft.Visibility = Visibility.Visible;
        bottomRight.Visibility = Visibility.Visible;

        PositionThumbs();
    }

    private void CreateCroppingRectangle()
    {
        croppingRectangle = new Rectangle
        {
            Stroke = Brushes.Blue,
            StrokeThickness = 2,
            Width = 100,
            Height = 100,
            Visibility = Visibility.Collapsed
        };

        OverlayCanvas.Children.Add(croppingRectangle);
        Canvas.SetLeft(croppingRectangle, 50);
        Canvas.SetTop(croppingRectangle, 50);
        croppingRectangle.MouseLeftButtonDown += CroppingRectangle_MouseLeftButtonDown;
        croppingRectangle.MouseMove += CroppingRectangle_MouseMove;
        croppingRectangle.MouseLeftButtonUp += CroppingRectangle_MouseLeftButtonUp;
    }

    private void AddThumbs()
    {
        topLeft = CreateThumb(Brushes.Red, Cursors.SizeNWSE);
        topRight = CreateThumb(Brushes.Red, Cursors.SizeNESW);
        bottomLeft = CreateThumb(Brushes.Red, Cursors.SizeNESW);
        bottomRight = CreateThumb(Brushes.Red, Cursors.SizeNWSE);

        topLeft.Visibility = Visibility.Collapsed;
        topRight.Visibility = Visibility.Collapsed;
        bottomLeft.Visibility = Visibility.Collapsed;
        bottomRight.Visibility = Visibility.Collapsed;

        AttachResizeHandler(topLeft, HorizontalAlignment.Left, VerticalAlignment.Top);
        AttachResizeHandler(topRight, HorizontalAlignment.Right, VerticalAlignment.Top);
        AttachResizeHandler(bottomLeft, HorizontalAlignment.Left, VerticalAlignment.Bottom);
        AttachResizeHandler(bottomRight, HorizontalAlignment.Right, VerticalAlignment.Bottom);

        OverlayCanvas.Children.Add(topLeft);
        OverlayCanvas.Children.Add(topRight);
        OverlayCanvas.Children.Add(bottomLeft);
        OverlayCanvas.Children.Add(bottomRight);
    }

    private void AttachResizeHandler(Thumb thumb, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
    {
        thumb.DragDelta += (sender, e) =>
        {
            if (sender is Thumb handle && croppingRectangle != null)
            {
                double originalWidth = croppingRectangle.Width;
                double originalHeight = croppingRectangle.Height;
                double newX = Canvas.GetLeft(croppingRectangle);
                double newY = Canvas.GetTop(croppingRectangle);

                if (horizontalAlignment == HorizontalAlignment.Right)
                {
                    double newWidth = originalWidth + e.HorizontalChange;
                    if (newWidth > 10)
                    {
                        croppingRectangle.Width = newWidth;
                    }
                }
                else if (horizontalAlignment == HorizontalAlignment.Left)
                {
                    double newWidth = originalWidth - e.HorizontalChange;
                    if (newWidth > 10)
                    {
                        newX += e.HorizontalChange;
                        if (newX >= 0)
                        {
                            croppingRectangle.Width = newWidth;
                            Canvas.SetLeft(croppingRectangle, newX);
                        }
                    }
                }

                if (verticalAlignment == VerticalAlignment.Bottom)
                {
                    double newHeight = originalHeight + e.VerticalChange;
                    if (newHeight > 10)
                    {
                        croppingRectangle.Height = newHeight;
                    }
                }
                else if (verticalAlignment == VerticalAlignment.Top)
                {
                    double newHeight = originalHeight - e.VerticalChange;
                    if (newHeight > 10)
                    {
                        newY += e.VerticalChange;
                        if (newY >= 0)
                        {
                            croppingRectangle.Height = newHeight;
                            Canvas.SetTop(croppingRectangle, newY);
                        }
                    }
                }
                PositionThumbs();
            }
        };
    }


    private Thumb CreateThumb(SolidColorBrush background, Cursor cursor)
    {
        return new Thumb
        {
            Width = 10,
            Height = 10,
            Background = background,
            Cursor = cursor
        };
    }

    private void PositionThumbs()
    {
        if (croppingRectangle != null)
        {
            Canvas.SetLeft(topLeft, Canvas.GetLeft(croppingRectangle) - topLeft.Width / 2);
            Canvas.SetTop(topLeft, Canvas.GetTop(croppingRectangle) - topLeft.Height / 2);

            Canvas.SetLeft(topRight, Canvas.GetLeft(croppingRectangle) + croppingRectangle.Width - topRight.Width / 2);
            Canvas.SetTop(topRight, Canvas.GetTop(croppingRectangle) - topRight.Height / 2);

            Canvas.SetLeft(bottomLeft, Canvas.GetLeft(croppingRectangle) - bottomLeft.Width / 2);
            Canvas.SetTop(bottomLeft, Canvas.GetTop(croppingRectangle) + croppingRectangle.Height - bottomLeft.Height / 2);

            Canvas.SetLeft(bottomRight, Canvas.GetLeft(croppingRectangle) + croppingRectangle.Width - bottomRight.Width / 2);
            Canvas.SetTop(bottomRight, Canvas.GetTop(croppingRectangle) + croppingRectangle.Height - bottomRight.Height / 2);
        }
    }

    private void CroppingRectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var rect = sender as Rectangle;
        rect.CaptureMouse();
        _isDragging = true;
        _startPoint = e.GetPosition(OverlayCanvas);
    }

    private void CroppingRectangle_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isDragging && sender is Rectangle rect)
        {
            Point currentPoint = e.GetPosition(OverlayCanvas);
            double offsetX = currentPoint.X - _startPoint.X;
            double offsetY = currentPoint.Y - _startPoint.Y;

            double newLeft = Math.Max(0, Canvas.GetLeft(rect) + offsetX);
            double newTop = Math.Max(0, Canvas.GetTop(rect) + offsetY);

            // Prevent the rectangle from moving outside the right boundary
            if (newLeft + rect.Width > OverlayCanvas.ActualWidth)
            {
                newLeft = OverlayCanvas.ActualWidth - rect.Width;
            }

            // Prevent the rectangle from moving outside the bottom boundary
            if (newTop + rect.Height > OverlayCanvas.ActualHeight)
            {
                newTop = OverlayCanvas.ActualHeight - rect.Height;
            }

            Canvas.SetLeft(rect, newLeft);
            Canvas.SetTop(rect, newTop);
            _startPoint = currentPoint;

            PositionThumbs();
        }
    }

    private void CroppingRectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        var rect = sender as Rectangle;
        rect.ReleaseMouseCapture();
        _isDragging = false;
    }
}