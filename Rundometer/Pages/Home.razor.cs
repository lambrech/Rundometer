namespace Rundometer.Pages;

using System.Diagnostics;

public partial class Home
{
    private string inputDistance = "1000";

    private string inputTimeMinutes = "4";

    private string inputTimeSeconds = "0";

    public string OutputInfoText { get; set; }

    public Stopwatch? Stopwatch { get; set; }

    public HashSet<(decimal, TimeSpan)> OutputCalculation { get; set; } = new();

    public List<(int Lap, TimeSpan Time)> Laps { get; set; } = new();

    public string InputDistance
    {
        get => this.inputDistance;

        set
        {
            if (this.inputDistance == value)
            {
                return;
            }

            this.inputDistance = value;
            this.UpdateOutput();
        }
    }

    public string InputTimeMinutes
    {
        get => this.inputTimeMinutes;

        set
        {
            if (this.inputTimeMinutes == value)
            {
                return;
            }

            this.inputTimeMinutes = value;
            this.UpdateOutput();
        }
    }

    public string InputTimeSeconds
    {
        get => this.inputTimeSeconds;

        set
        {
            if (this.inputTimeSeconds == value)
            {
                return;
            }

            this.inputTimeSeconds = value;
            this.UpdateOutput();
        }
    }

    public void UpdateOutput()
    {
        this.OutputInfoText = string.Empty;
        this.OutputCalculation = new HashSet<(decimal, TimeSpan)>();

        if (!decimal.TryParse(this.InputDistance, out var distance) || (distance < 100))
        {
            this.OutputInfoText = "Fehlerhafte Zahleneingabe im 'Distanz'-Feld";
            return;
        }

        if (!decimal.TryParse(this.InputTimeMinutes, out var minutes) || (minutes < 0))
        {
            this.OutputInfoText = "Fehlerhafte Zahleneingabe im 'Minuten'-Feld";
            return;
        }

        if (!decimal.TryParse(this.InputTimeSeconds, out var seconds) || (seconds < 0))
        {
            this.OutputInfoText = "Fehlerhafte Zahleneingabe im 'Sekunden'-Feld";
            return;
        }

        var time = TimeSpan.FromMinutes((double)minutes).Add(TimeSpan.FromSeconds((double)seconds));

        if (time.TotalSeconds <= 0)
        {
            this.OutputInfoText = "Gesamtzeit muss größer als 0 sein";
            return;
        }

        var distanceParts = new HashSet<(decimal, TimeSpan)>();
        var secondsPerMeter = (decimal)time.TotalSeconds / distance;
        decimal tmp = 100;

        while (tmp <= distance)
        {
            distanceParts.Add((tmp, TimeSpan.FromSeconds((double)(tmp * secondsPerMeter))));
            tmp += 100;
        }

        distanceParts.Add((distance, time));

        this.OutputCalculation = distanceParts;
    }

    protected override Task OnInitializedAsync()
    {
        this.UpdateOutput();
        return Task.CompletedTask;
    }

    private async Task<IEnumerable<string>> DistanceSearch(string arg)
    {
        await Task.Delay(0);
        var all = new[] { "1500", "1000", "800", arg }.ToHashSet();
        // return all.Where(x => x.ToLowerInvariant().Contains(arg)).ToList();
        return all;
    }

    private void OnStartLapClick()
    {
        if (this.Stopwatch is { IsRunning: true })
        {
            this.Laps.Add((this.Laps.Count + 1, this.Stopwatch.Elapsed));
        }
        else
        {
            this.Laps = new List<(int Lap, TimeSpan Time)>();
            this.Stopwatch = Stopwatch.StartNew();

            Task.Run(
                async () =>
                {
                    var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
                    while (this.Stopwatch.IsRunning)
                    {
                        await timer.WaitForNextTickAsync();
                        this.StateHasChanged();
                    }

                    timer.Dispose();
                });
        }
    }

    private void OnStopClick()
    {
        if (this.Stopwatch == null)
        {
            return;
        }

        this.Stopwatch.Stop();
    }
}