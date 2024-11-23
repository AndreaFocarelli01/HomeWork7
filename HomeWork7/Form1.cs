using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HomeWork7
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Graphics theoretical = pictureBox1.CreateGraphics();
            theoretical.Clear(Color.White);

            Graphics empirical = pictureBox2.CreateGraphics();
            empirical.Clear(Color.White);

            //data 
            int m = int.Parse(textBox1.Text);           //number of samples
            int n = int.Parse(textBox3.Text);           //sample size
            int interval = int.Parse(textBox2.Text);    //intervals size

            float width = pictureBox1.Width;
            float height = pictureBox1.Height;

            double[] prob = GenerateProbabilities(interval);
            double theoreticalMean = RecursiveMean(prob, interval - 1, 0, 0);
            double theoreticalVariance = RecursiveVariance(prob, interval - 1, theoreticalMean, 0);


            List<double> samplingMean = new List<double>();
            Random rand = new Random();

            for(int i = 0; i < m; i++)
            {
                samplingMean.Add(GenerateSampleMean(prob, n, interval, rand));
            }

            int[] empiricalDistribution = new int[interval];
            foreach (double mean in samplingMean)
            {
                empiricalDistribution[(int)mean]++;
            }
            double samplingMeanAverage = samplingMean.Average();
            double samplingMeanVariance = samplingMean.Select(x => Math.Pow(x - samplingMeanAverage, 2)).Average();

            DrawHistogramForMeans(empirical, samplingMean, width, height, new Pen(Color.Green),
                samplingMeanAverage, samplingMeanVariance, "Distribution of Sample Means");

            DrawHistogramTheoretical(theoretical, empiricalDistribution, m, interval, height, width, new Pen(Color.Blue),
                theoreticalMean, theoreticalVariance);
        }

        private double[] GenerateProbabilities(int intervals)
        {
            double[] probabilities = new double[intervals];
            Random random = new Random();
            double sum = 0;

            for (int i = 0; i < intervals - 1; i++)
            {
                double remaining = 1 - sum;
                double prob = random.NextDouble() * remaining * 0.8;
                probabilities[i] = prob;
                sum += prob;
            }
            probabilities[intervals - 1] = 1 - sum; // Ensure total sums to 1

            return probabilities;
        }

        private double GenerateSampleMean(double[] probabilities, int sampleSize, int intervals, Random random)
        {
            double sampleSum = 0;
            for (int i = 0; i < sampleSize; i++)
            {
                double rand = random.NextDouble();
                double cumulative = 0;
                for (int j = 0; j < intervals; j++)
                {
                    cumulative += probabilities[j];
                    if (rand <= cumulative)
                    {
                        sampleSum += j;
                        break;
                    }
                }
            }
            return sampleSum / sampleSize;
        }

        private void DrawHistogramForMeans(Graphics histogram, List<double> sampleMeans, float width, float height, Pen pen,
    double mean, double variance, string title)
        {
            int bins = 20; // Number of bins for the histogram
            double min = sampleMeans.Min();
            double max = sampleMeans.Max();
            double binWidth = (max - min) / bins;

            int[] frequencies = new int[bins];
            foreach (var value in sampleMeans)
            {
                int bin = Math.Min((int)((value - min) / binWidth), bins - 1);
                frequencies[bin]++;
            }

            float barWidth = (width / bins) - 2;
            float maxFrequency = frequencies.Max();
            histogram.Clear(Color.White);

            for (int i = 0; i < bins; i++)
            {
                float barHeight = (float)(frequencies[i] / maxFrequency * height);
                histogram.FillRectangle(pen.Brush, i * (barWidth + 2), height - barHeight, barWidth, barHeight);
            }

            DrawTitleAndLabels(histogram, bins, height, barWidth, title, mean, variance);
        }

        private void DrawHistogramTheoretical(Graphics g, int[] frequencies, int totalSamples, int intervals,
    float height, float width, Pen pen, double mean, double variance)
        {
            // Calculate proportions from frequencies
            double[] proportions = frequencies.Select(f => (double)f / totalSamples).ToArray();

            // Determine the maximum proportion for scaling
            double maxProportion = proportions.Max();

            // Calculate the bar width, leaving small spacing between bars
            float barWidth = (width / intervals) - 2;

            // Clear the drawing area
            g.Clear(Color.White);
            pen.Color = Color.Blue;
            pen.Width = 2;

            // Draw histogram bars
            for (int i = 0; i < intervals; i++)
            {
                // Scale bar height to fit within the available height
                float barHeight = (float)(proportions[i] / maxProportion * height);

                // Draw the bar
                g.FillRectangle(pen.Brush,
                    i * (barWidth + 2),      // X-position (with spacing)
                    height - barHeight,      // Y-position (inverted coordinate system)
                    barWidth,                // Bar width
                    barHeight);              // Bar height

                // Add text label above each bar to show the proportion as a percentage
                using (Font font = new Font("Arial", 8))
                {
                    string valueText = $"{(proportions[i] * 100):F1}%";
                    SizeF textSize = g.MeasureString(valueText, font);
                    float textX = i * (barWidth + 2) + (barWidth - textSize.Width) / 2; // Centered text
                    float textY = height - barHeight - textSize.Height - 2;             // Position above bar
                    g.DrawString(valueText, font, Brushes.Black, textX, textY);
                }
            }
        }


        private double RecursiveMean(double[] probabilities, int index, int value, double currentMean)
        {
            if (index < 0) return currentMean; // Base case
            return RecursiveMean(probabilities, index - 1, value + 1, currentMean + value * probabilities[index]);
        }

        private double RecursiveVariance(double[] probabilities, int index, double mean, double currentVariance)
        {
            if (index < 0) return currentVariance; // Base case
            double deviation = index - mean;
            return RecursiveVariance(probabilities, index - 1, mean, currentVariance + deviation * deviation * probabilities[index]);
        }

        private void DrawTitleAndLabels(Graphics g, int intervals, float height, float barWidth,
    string title, double mean, double variance)
        {
            using (Pen axisPen = new Pen(Color.Black, 2))
            {
                // Draw the X-axis
                g.DrawLine(axisPen, 0, height, intervals * (barWidth + 2), height);

                // Draw the Y-axis
                g.DrawLine(axisPen, 0, 0, 0, height);

                // Draw Y-axis labels and tick marks
                for (int i = 0; i <= 10; i++) // 10 divisions on the Y-axis
                {
                    float y = height * (1 - i / 10.0f); // Position of the tick mark
                    g.DrawLine(axisPen, -5, y, 5, y); // Tick mark

                    using (Font font = new Font("Arial", 8))
                    {
                        g.DrawString($"{i * 10}%", font, Brushes.Black, -30, y - 6);
                    }
                }

                // Draw X-axis labels for bins
                using (Font font = new Font("Arial", 8))
                {
                    for (int i = 0; i < intervals; i++)
                    {
                        float x = i * (barWidth + 2) + barWidth / 2 - 10;
                        g.DrawString(i.ToString(), font, Brushes.Black, x, height + 5);
                    }
                }

                // Draw the title and statistics
                using (Font titleFont = new Font("Arial", 10, FontStyle.Bold))
                {
                    string[] titleLines = title.Split('\n');
                    float y = 10; // Start drawing title at the top

                    foreach (string line in titleLines)
                    {
                        g.DrawString(line, titleFont, Brushes.Black, intervals * (barWidth + 2) / 2 - 75, y);
                        y += 20; // Space between lines
                    }

                    // Add mean and variance details below the title
                    g.DrawString($"Mean: {mean:F2}, Variance: {variance:F2}", titleFont, Brushes.Black,
                        intervals * (barWidth + 2) / 2 - 75, y);
                }
            }
        }

    }
}
