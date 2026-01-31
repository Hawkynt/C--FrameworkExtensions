#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
//
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
//
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
//
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace System.Windows.Forms.Charting.Renderers;

/// <summary>
/// Renderer for candlestick charts.
/// </summary>
public class CandlestickRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Candlestick;

  /// <inheritdoc />
  public override bool UsesAxes => true;

  /// <summary>
  /// Color for bullish (up) candles.
  /// </summary>
  public Color BullishColor { get; set; } = Color.FromArgb(38, 166, 91);

  /// <summary>
  /// Color for bearish (down) candles.
  /// </summary>
  public Color BearishColor { get; set; } = Color.FromArgb(231, 76, 60);

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var ohlcData = context.Chart.OHLCData;

    if (ohlcData == null || ohlcData.Count == 0)
      return;

    var candleCount = ohlcData.Count;
    var candleWidth = context.PlotArea.Width / candleCount * 0.7f;
    var gap = context.PlotArea.Width / candleCount * 0.15f;

    for (var i = 0; i < ohlcData.Count; ++i) {
      var data = ohlcData[i];
      var x = context.PlotArea.Left + i * (candleWidth + gap * 2) + gap;
      var centerX = x + candleWidth / 2;

      var openY = ValueToPixelY(context, data.Open);
      var highY = ValueToPixelY(context, data.High);
      var lowY = ValueToPixelY(context, data.Low);
      var closeY = ValueToPixelY(context, data.Close);

      // Apply animation
      if (context.AnimationProgress < 1) {
        var midY = (openY + closeY) / 2;
        openY = midY + (float)((openY - midY) * context.AnimationProgress);
        highY = midY + (float)((highY - midY) * context.AnimationProgress);
        lowY = midY + (float)((lowY - midY) * context.AnimationProgress);
        closeY = midY + (float)((closeY - midY) * context.AnimationProgress);
      }

      var isBullish = data.Close >= data.Open;
      var color = isBullish ? this.BullishColor : this.BearishColor;

      // Draw wick (high-low line)
      using (var pen = new Pen(color, 1))
        g.DrawLine(pen, centerX, highY, centerX, lowY);

      // Draw candle body
      var bodyTop = Math.Min(openY, closeY);
      var bodyHeight = Math.Max(Math.Abs(closeY - openY), 1);

      if (isBullish) {
        // Hollow candle for bullish
        using (var pen = new Pen(color, 2))
          g.DrawRectangle(pen, x, bodyTop, candleWidth, bodyHeight);
      } else {
        // Filled candle for bearish
        using (var brush = new SolidBrush(color))
          g.FillRectangle(brush, x, bodyTop, candleWidth, bodyHeight);
      }
    }
  }
}

/// <summary>
/// Renderer for OHLC (Open-High-Low-Close) bar charts.
/// </summary>
public class OHLCRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.OHLC;

  /// <inheritdoc />
  public override bool UsesAxes => true;

  /// <summary>
  /// Color for bullish (up) bars.
  /// </summary>
  public Color BullishColor { get; set; } = Color.FromArgb(38, 166, 91);

  /// <summary>
  /// Color for bearish (down) bars.
  /// </summary>
  public Color BearishColor { get; set; } = Color.FromArgb(231, 76, 60);

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var ohlcData = context.Chart.OHLCData;

    if (ohlcData == null || ohlcData.Count == 0)
      return;

    var barCount = ohlcData.Count;
    var barSpacing = context.PlotArea.Width / barCount;
    var tickWidth = barSpacing * 0.3f;

    for (var i = 0; i < ohlcData.Count; ++i) {
      var data = ohlcData[i];
      var centerX = context.PlotArea.Left + (i + 0.5f) * barSpacing;

      var openY = ValueToPixelY(context, data.Open);
      var highY = ValueToPixelY(context, data.High);
      var lowY = ValueToPixelY(context, data.Low);
      var closeY = ValueToPixelY(context, data.Close);

      // Apply animation
      if (context.AnimationProgress < 1) {
        var midY = (highY + lowY) / 2;
        openY = midY + (float)((openY - midY) * context.AnimationProgress);
        highY = midY + (float)((highY - midY) * context.AnimationProgress);
        lowY = midY + (float)((lowY - midY) * context.AnimationProgress);
        closeY = midY + (float)((closeY - midY) * context.AnimationProgress);
      }

      var isBullish = data.Close >= data.Open;
      var color = isBullish ? this.BullishColor : this.BearishColor;

      using var pen = new Pen(color, 2);

      // Draw vertical line (high to low)
      g.DrawLine(pen, centerX, highY, centerX, lowY);

      // Draw open tick (left)
      g.DrawLine(pen, centerX - tickWidth, openY, centerX, openY);

      // Draw close tick (right)
      g.DrawLine(pen, centerX, closeY, centerX + tickWidth, closeY);
    }
  }
}

/// <summary>
/// Renderer for waterfall charts.
/// </summary>
public class WaterfallChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Waterfall;

  /// <inheritdoc />
  public override bool SupportsMultipleSeries => false;

  /// <summary>
  /// Color for positive values.
  /// </summary>
  public Color PositiveColor { get; set; } = Color.FromArgb(38, 166, 91);

  /// <summary>
  /// Color for negative values.
  /// </summary>
  public Color NegativeColor { get; set; } = Color.FromArgb(231, 76, 60);

  /// <summary>
  /// Color for total/subtotal bars.
  /// </summary>
  public Color TotalColor { get; set; } = Color.FromArgb(52, 73, 94);

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    // Use WaterfallData if available, otherwise use series data
    var waterfallData = context.Chart.WaterfallData;
    if (waterfallData != null && waterfallData.Count > 0)
      this._RenderFromWaterfallData(context, waterfallData);
    else
      this._RenderFromSeriesData(context);
  }

  private void _RenderFromWaterfallData(ChartRenderContext context, IList<WaterfallStep> steps) {
    var g = context.Graphics;
    var barCount = steps.Count;
    var barWidth = context.PlotArea.Width / barCount * 0.7f;
    var gap = context.PlotArea.Width / barCount * 0.15f;

    var runningTotal = 0.0;
    var baseY = ValueToPixelY(context, 0);

    for (var i = 0; i < steps.Count; ++i) {
      var step = steps[i];
      var x = context.PlotArea.Left + i * (barWidth + gap * 2) + gap;

      float topY, bottomY;
      Color color;

      if (step.IsTotal) {
        // Total bar: from 0 to running total
        bottomY = baseY;
        topY = ValueToPixelY(context, runningTotal);
        color = step.Color ?? this.TotalColor;
      } else {
        // Regular step: from previous total to new total
        var previousTotal = runningTotal;
        runningTotal += step.Value;

        var previousY = ValueToPixelY(context, previousTotal);
        var currentY = ValueToPixelY(context, runningTotal);

        if (step.Value >= 0) {
          topY = currentY;
          bottomY = previousY;
          color = step.Color ?? this.PositiveColor;
        } else {
          topY = previousY;
          bottomY = currentY;
          color = step.Color ?? this.NegativeColor;
        }
      }

      // Apply animation
      if (context.AnimationProgress < 1) {
        var midY = (topY + bottomY) / 2;
        topY = midY + (float)((topY - midY) * context.AnimationProgress);
        bottomY = midY + (float)((bottomY - midY) * context.AnimationProgress);
      }

      var barHeight = Math.Abs(bottomY - topY);
      var barTop = Math.Min(topY, bottomY);

      // Draw bar
      using (var brush = new SolidBrush(color))
        g.FillRectangle(brush, x, barTop, barWidth, Math.Max(barHeight, 1));

      using (var pen = new Pen(Darken(color, 0.2f), 1))
        g.DrawRectangle(pen, x, barTop, barWidth, Math.Max(barHeight, 1));

      // Draw connector line to next bar (if not last and not a total)
      if (i < steps.Count - 1 && !step.IsTotal) {
        var connectorY = step.Value >= 0 ? topY : bottomY;
        using var pen = new Pen(Color.Gray, 1) { DashStyle = DashStyle.Dash };
        g.DrawLine(pen, x + barWidth, connectorY, x + barWidth + gap * 2, connectorY);
      }

      // Draw label
      if (!string.IsNullOrEmpty(step.Label)) {
        var labelSize = g.MeasureString(step.Label, context.Chart.Font);
        using var brush = new SolidBrush(Color.Black);
        g.DrawString(step.Label, context.Chart.Font, brush, x + barWidth / 2 - labelSize.Width / 2, context.PlotArea.Bottom + 5);
      }
    }
  }

  private void _RenderFromSeriesData(ChartRenderContext context) {
    var series = context.Series.FirstOrDefault();
    if (series == null || series.Points.Count == 0)
      return;

    var g = context.Graphics;
    var barCount = series.Points.Count;
    var barWidth = context.PlotArea.Width / barCount * 0.7f;
    var gap = context.PlotArea.Width / barCount * 0.15f;

    var runningTotal = 0.0;
    var baseY = ValueToPixelY(context, 0);

    for (var i = 0; i < series.Points.Count; ++i) {
      var dp = series.Points[i];
      var x = context.PlotArea.Left + i * (barWidth + gap * 2) + gap;

      var previousTotal = runningTotal;
      runningTotal += dp.Y;

      var previousY = ValueToPixelY(context, previousTotal);
      var currentY = ValueToPixelY(context, runningTotal);

      float topY, bottomY;
      Color color;

      if (dp.Y >= 0) {
        topY = currentY;
        bottomY = previousY;
        color = dp.Color ?? this.PositiveColor;
      } else {
        topY = previousY;
        bottomY = currentY;
        color = dp.Color ?? this.NegativeColor;
      }

      // Apply animation
      if (context.AnimationProgress < 1) {
        var midY = (topY + bottomY) / 2;
        topY = midY + (float)((topY - midY) * context.AnimationProgress);
        bottomY = midY + (float)((bottomY - midY) * context.AnimationProgress);
      }

      var barHeight = Math.Abs(bottomY - topY);
      var barTop = Math.Min(topY, bottomY);

      // Draw bar
      using (var brush = new SolidBrush(color))
        g.FillRectangle(brush, x, barTop, barWidth, Math.Max(barHeight, 1));

      using (var pen = new Pen(Darken(color, 0.2f), 1))
        g.DrawRectangle(pen, x, barTop, barWidth, Math.Max(barHeight, 1));

      context.RegisterHitTestRect(dp, new RectangleF(x, barTop, barWidth, barHeight));

      // Draw connector line
      if (i < series.Points.Count - 1) {
        var connectorY = dp.Y >= 0 ? topY : bottomY;
        using var pen = new Pen(Color.Gray, 1) { DashStyle = DashStyle.Dash };
        g.DrawLine(pen, x + barWidth, connectorY, x + barWidth + gap * 2, connectorY);
      }

      // Data label
      if (context.ShowDataLabels) {
        var label = dp.Label ?? dp.Y.ToString("N1");
        DrawDataLabel(g, label, new PointF(x + barWidth / 2, barTop), context.Chart.Font, Color.Black, context.DataLabelPosition);
      }
    }
  }
}

/// <summary>
/// Renderer for Kagi charts.
/// </summary>
public class KagiChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Kagi;

  /// <inheritdoc />
  public override bool SupportsMultipleSeries => false;

  /// <summary>
  /// Color for yang (thick/bullish) lines.
  /// </summary>
  public Color YangColor { get; set; } = Color.FromArgb(38, 166, 91);

  /// <summary>
  /// Color for yin (thin/bearish) lines.
  /// </summary>
  public Color YinColor { get; set; } = Color.FromArgb(231, 76, 60);

  /// <summary>
  /// Reversal amount (percentage or absolute).
  /// </summary>
  public double ReversalAmount { get; set; } = 4.0;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var series = context.Series.FirstOrDefault();

    if (series == null || series.Points.Count < 2)
      return;

    // Sort points by X (time)
    var sortedPoints = series.Points.OrderBy(p => p.X).ToList();

    // Calculate Kagi lines
    var kagiLines = this._CalculateKagiLines(sortedPoints, context);

    if (kagiLines.Count < 2)
      return;

    // Draw Kagi lines
    for (var i = 1; i < kagiLines.Count; ++i) {
      var prev = kagiLines[i - 1];
      var curr = kagiLines[i];

      // Apply animation
      var animatedPrevY = prev.Y;
      var animatedCurrY = curr.Y;
      if (context.AnimationProgress < 1) {
        var midY = context.PlotArea.Top + context.PlotArea.Height / 2;
        animatedPrevY = midY + (float)((prev.Y - midY) * context.AnimationProgress);
        animatedCurrY = midY + (float)((curr.Y - midY) * context.AnimationProgress);
      }

      var isYang = curr.Y < prev.Y; // Price going up (remember Y is inverted)
      var color = isYang ? this.YangColor : this.YinColor;
      var lineWidth = isYang ? 3f : 1f;

      using var pen = new Pen(color, lineWidth);

      if (Math.Abs(prev.X - curr.X) < 0.001) {
        // Vertical line
        g.DrawLine(pen, prev.X, animatedPrevY, curr.X, animatedCurrY);
      } else {
        // Horizontal line with vertical connection
        g.DrawLine(pen, prev.X, animatedPrevY, curr.X, animatedPrevY);
        g.DrawLine(pen, curr.X, animatedPrevY, curr.X, animatedCurrY);
      }
    }
  }

  private List<PointF> _CalculateKagiLines(IList<ChartPoint> points, ChartRenderContext context) {
    var result = new List<PointF>();
    if (points.Count == 0)
      return result;

    var xStep = context.PlotArea.Width / Math.Max(points.Count, 1);
    var currentX = context.PlotArea.Left;

    var direction = 0; // 1 = up, -1 = down, 0 = unknown
    var lastReversePrice = points[0].Y;
    var currentPrice = points[0].Y;

    result.Add(new PointF(currentX, ValueToPixelY(context, currentPrice)));

    for (var i = 1; i < points.Count; ++i) {
      var price = points[i].Y;
      var change = price - currentPrice;
      var reverseThreshold = lastReversePrice * this.ReversalAmount / 100;

      if (direction == 0) {
        // Determine initial direction
        direction = change > 0 ? 1 : (change < 0 ? -1 : 0);
        if (direction != 0) {
          currentPrice = price;
          result.Add(new PointF(currentX, ValueToPixelY(context, currentPrice)));
        }
      } else if (direction == 1) {
        // Currently going up
        if (price > currentPrice) {
          // Continue up
          currentPrice = price;
          if (result.Count > 0)
            result[result.Count - 1] = new PointF(currentX, ValueToPixelY(context, currentPrice));
        } else if (currentPrice - price >= reverseThreshold) {
          // Reverse down
          currentX += xStep;
          direction = -1;
          lastReversePrice = currentPrice;
          currentPrice = price;
          result.Add(new PointF(currentX, ValueToPixelY(context, currentPrice)));
        }
      } else {
        // Currently going down
        if (price < currentPrice) {
          // Continue down
          currentPrice = price;
          if (result.Count > 0)
            result[result.Count - 1] = new PointF(currentX, ValueToPixelY(context, currentPrice));
        } else if (price - currentPrice >= reverseThreshold) {
          // Reverse up
          currentX += xStep;
          direction = 1;
          lastReversePrice = currentPrice;
          currentPrice = price;
          result.Add(new PointF(currentX, ValueToPixelY(context, currentPrice)));
        }
      }
    }

    return result;
  }
}

/// <summary>
/// Renderer for Renko charts.
/// </summary>
public class RenkoChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Renko;

  /// <inheritdoc />
  public override bool SupportsMultipleSeries => false;

  /// <summary>
  /// Color for bullish (up) bricks.
  /// </summary>
  public Color BullishColor { get; set; } = Color.FromArgb(38, 166, 91);

  /// <summary>
  /// Color for bearish (down) bricks.
  /// </summary>
  public Color BearishColor { get; set; } = Color.FromArgb(231, 76, 60);

  /// <summary>
  /// Brick size (price movement needed to create a new brick).
  /// </summary>
  public double BrickSize { get; set; } = 0;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var series = context.Series.FirstOrDefault();

    if (series == null || series.Points.Count < 2)
      return;

    // Sort points by X (time)
    var sortedPoints = series.Points.OrderBy(p => p.X).ToList();

    // Calculate brick size if not specified
    var brickSize = this.BrickSize;
    if (brickSize <= 0) {
      var prices = sortedPoints.Select(p => p.Y).ToList();
      var range = prices.Max() - prices.Min();
      brickSize = range / 20; // Default: divide range into ~20 bricks
    }

    // Calculate Renko bricks
    var bricks = this._CalculateRenkoBricks(sortedPoints, brickSize);

    if (bricks.Count == 0)
      return;

    var brickWidth = context.PlotArea.Width / Math.Max(bricks.Count, 1) * 0.9f;

    for (var i = 0; i < bricks.Count; ++i) {
      var brick = bricks[i];
      var x = context.PlotArea.Left + i * (brickWidth + brickWidth * 0.1f);

      var topY = ValueToPixelY(context, brick.Top);
      var bottomY = ValueToPixelY(context, brick.Bottom);

      // Apply animation
      if (context.AnimationProgress < 1) {
        var midY = (topY + bottomY) / 2;
        topY = midY + (float)((topY - midY) * context.AnimationProgress);
        bottomY = midY + (float)((bottomY - midY) * context.AnimationProgress);
      }

      var color = brick.IsBullish ? this.BullishColor : this.BearishColor;
      var brickHeight = Math.Abs(bottomY - topY);
      var brickTop = Math.Min(topY, bottomY);

      // Draw brick
      using (var brush = new SolidBrush(color))
        g.FillRectangle(brush, x, brickTop, brickWidth, brickHeight);

      using (var pen = new Pen(Darken(color, 0.2f), 1))
        g.DrawRectangle(pen, x, brickTop, brickWidth, brickHeight);
    }
  }

  private List<RenkoBrick> _CalculateRenkoBricks(IList<ChartPoint> points, double brickSize) {
    var bricks = new List<RenkoBrick>();
    if (points.Count == 0 || brickSize <= 0)
      return bricks;

    var currentPrice = points[0].Y;
    var lastBrickTop = Math.Ceiling(currentPrice / brickSize) * brickSize;
    var lastBrickBottom = lastBrickTop - brickSize;
    var lastDirection = 0; // 1 = up, -1 = down

    for (var i = 1; i < points.Count; ++i) {
      var price = points[i].Y;

      if (lastDirection >= 0 && price >= lastBrickTop + brickSize) {
        // Add bullish bricks
        while (price >= lastBrickTop + brickSize) {
          bricks.Add(new RenkoBrick {
            Top = lastBrickTop + brickSize,
            Bottom = lastBrickTop,
            IsBullish = true
          });
          lastBrickBottom = lastBrickTop;
          lastBrickTop += brickSize;
          lastDirection = 1;
        }
      } else if (lastDirection <= 0 && price <= lastBrickBottom - brickSize) {
        // Add bearish bricks
        while (price <= lastBrickBottom - brickSize) {
          bricks.Add(new RenkoBrick {
            Top = lastBrickBottom,
            Bottom = lastBrickBottom - brickSize,
            IsBullish = false
          });
          lastBrickTop = lastBrickBottom;
          lastBrickBottom -= brickSize;
          lastDirection = -1;
        }
      } else if (lastDirection == 1 && price <= lastBrickBottom - brickSize) {
        // Reversal from up to down
        while (price <= lastBrickBottom - brickSize) {
          bricks.Add(new RenkoBrick {
            Top = lastBrickBottom,
            Bottom = lastBrickBottom - brickSize,
            IsBullish = false
          });
          lastBrickTop = lastBrickBottom;
          lastBrickBottom -= brickSize;
          lastDirection = -1;
        }
      } else if (lastDirection == -1 && price >= lastBrickTop + brickSize) {
        // Reversal from down to up
        while (price >= lastBrickTop + brickSize) {
          bricks.Add(new RenkoBrick {
            Top = lastBrickTop + brickSize,
            Bottom = lastBrickTop,
            IsBullish = true
          });
          lastBrickBottom = lastBrickTop;
          lastBrickTop += brickSize;
          lastDirection = 1;
        }
      }
    }

    return bricks;
  }

  private class RenkoBrick {
    public double Top { get; set; }
    public double Bottom { get; set; }
    public bool IsBullish { get; set; }
  }
}

/// <summary>
/// Renderer for Point and Figure charts.
/// </summary>
public class PointFigureRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.PointFigure;

  /// <inheritdoc />
  public override bool SupportsMultipleSeries => false;

  /// <summary>
  /// Color for X marks (bullish).
  /// </summary>
  public Color XColor { get; set; } = Color.FromArgb(38, 166, 91);

  /// <summary>
  /// Color for O marks (bearish).
  /// </summary>
  public Color OColor { get; set; } = Color.FromArgb(231, 76, 60);

  /// <summary>
  /// Box size (price unit per box).
  /// </summary>
  public double BoxSize { get; set; } = 0;

  /// <summary>
  /// Reversal amount (number of boxes needed for reversal).
  /// </summary>
  public int ReversalBoxes { get; set; } = 3;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var series = context.Series.FirstOrDefault();

    if (series == null || series.Points.Count < 2)
      return;

    // Sort points by X (time)
    var sortedPoints = series.Points.OrderBy(p => p.X).ToList();

    // Calculate box size if not specified
    var boxSize = this.BoxSize;
    if (boxSize <= 0) {
      var prices = sortedPoints.Select(p => p.Y).ToList();
      var range = prices.Max() - prices.Min();
      boxSize = range / 30;
    }

    // Calculate P&F columns
    var columns = this._CalculatePnFColumns(sortedPoints, boxSize);

    if (columns.Count == 0)
      return;

    var columnWidth = context.PlotArea.Width / Math.Max(columns.Count, 1);
    var boxHeight = context.PlotArea.Height / 30; // Approximate

    for (var col = 0; col < columns.Count; ++col) {
      var column = columns[col];
      var x = context.PlotArea.Left + col * columnWidth + columnWidth / 2;
      var symbolSize = Math.Min(columnWidth, boxHeight) * 0.8f;

      var color = column.IsX ? this.XColor : this.OColor;

      for (var box = column.StartBox; box <= column.EndBox; ++box) {
        var y = ValueToPixelY(context, box * boxSize);

        // Apply animation
        if (context.AnimationProgress < 1) {
          var targetY = y;
          var startY = context.PlotArea.Top + context.PlotArea.Height / 2;
          y = startY + (float)((targetY - startY) * context.AnimationProgress);
        }

        if (column.IsX)
          this._DrawX(g, x, y, symbolSize, color);
        else
          this._DrawO(g, x, y, symbolSize, color);
      }
    }
  }

  private void _DrawX(Graphics g, float x, float y, float size, Color color) {
    var half = size / 2;
    using var pen = new Pen(color, 2);
    g.DrawLine(pen, x - half, y - half, x + half, y + half);
    g.DrawLine(pen, x - half, y + half, x + half, y - half);
  }

  private void _DrawO(Graphics g, float x, float y, float size, Color color) {
    var half = size / 2;
    using var pen = new Pen(color, 2);
    g.DrawEllipse(pen, x - half, y - half, size, size);
  }

  private List<PnFColumn> _CalculatePnFColumns(IList<ChartPoint> points, double boxSize) {
    var columns = new List<PnFColumn>();
    if (points.Count == 0 || boxSize <= 0)
      return columns;

    var currentBox = (int)Math.Floor(points[0].Y / boxSize);
    var currentColumn = new PnFColumn { IsX = true, StartBox = currentBox, EndBox = currentBox };
    var lastDirection = 0; // 1 = up (X), -1 = down (O)

    for (var i = 1; i < points.Count; ++i) {
      var newBox = (int)Math.Floor(points[i].Y / boxSize);

      if (lastDirection >= 0 && newBox > currentColumn.EndBox) {
        // Continue X column up
        currentColumn.EndBox = newBox;
        lastDirection = 1;
      } else if (lastDirection <= 0 && newBox < currentColumn.StartBox) {
        // Continue O column down
        currentColumn.StartBox = newBox;
        lastDirection = -1;
      } else if (lastDirection == 1 && currentColumn.EndBox - newBox >= this.ReversalBoxes) {
        // Reversal from X to O
        columns.Add(currentColumn);
        currentColumn = new PnFColumn { IsX = false, StartBox = newBox, EndBox = currentColumn.EndBox - 1 };
        lastDirection = -1;
      } else if (lastDirection == -1 && newBox - currentColumn.StartBox >= this.ReversalBoxes) {
        // Reversal from O to X
        columns.Add(currentColumn);
        currentColumn = new PnFColumn { IsX = true, StartBox = currentColumn.StartBox + 1, EndBox = newBox };
        lastDirection = 1;
      } else if (lastDirection == 0) {
        // Initial direction
        if (newBox > currentBox) {
          currentColumn.EndBox = newBox;
          lastDirection = 1;
        } else if (newBox < currentBox) {
          currentColumn.IsX = false;
          currentColumn.StartBox = newBox;
          lastDirection = -1;
        }
      }

      currentBox = newBox;
    }

    columns.Add(currentColumn);
    return columns;
  }

  private class PnFColumn {
    public bool IsX { get; set; }
    public int StartBox { get; set; }
    public int EndBox { get; set; }
  }
}
