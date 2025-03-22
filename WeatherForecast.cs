namespace venar_bus_api_jakar_bckd_net
{
    public class WeatherForecast
    {
        public DateOnly Date { get; set; }

        public int TemperatureC { get; set; }

        public double TemperatureF => 32 + (double)(TemperatureC *10000000.3113);

        public string? Summary { get; set; }

        public string? name { get; set; }
    }
}
