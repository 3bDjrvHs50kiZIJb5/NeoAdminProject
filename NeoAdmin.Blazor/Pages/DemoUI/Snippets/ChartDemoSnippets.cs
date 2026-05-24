namespace NeoAdmin.Blazor.Pages.DemoUI;

internal static class ChartDemoSnippets
{
    public const string Overview = """
        <ChartContainer Height="300">
            <BarChart Data="@chartData">
                <Grid Vertical="false" />
                <XAxis DataKey="month" />
                <YAxis />
                <ChartTooltip />
                <Legend />
                <Bar DataKey="desktop" Name="Desktop" Color="var(--chart-1)" />
                <Bar DataKey="mobile" Name="Mobile" Color="var(--chart-2)" />
            </BarChart>
        </ChartContainer>
        """;

    public const string Area = """
        <ChartContainer Height="300">
            <AreaChart Data="@chartData">
                <Grid Vertical="false" />
                <XAxis DataKey="month" />
                <YAxis />
                <ChartTooltip />
                <Legend />
                <Area DataKey="desktop" Name="Desktop" Color="var(--chart-1)" />
                <Area DataKey="mobile" Name="Mobile" Color="var(--chart-2)" />
            </AreaChart>
        </ChartContainer>
        """;

    public const string Bar = """
        <ChartContainer Height="300">
            <BarChart Data="@chartData">
                <Grid Vertical="false" />
                <XAxis DataKey="month" />
                <YAxis />
                <ChartTooltip />
                <Legend />
                <Bar DataKey="desktop" Name="Desktop" Color="var(--chart-1)" StackId="a" />
                <Bar DataKey="mobile" Name="Mobile" Color="var(--chart-2)" StackId="a" />
            </BarChart>
        </ChartContainer>
        """;

    public const string Line = """
        <ChartContainer Height="300">
            <LineChart Data="@chartData">
                <Grid Vertical="false" />
                <XAxis DataKey="month" />
                <YAxis />
                <ChartTooltip />
                <Legend />
                <Line DataKey="desktop" Name="Desktop" Color="var(--chart-1)" ShowDots="true" />
                <Line DataKey="mobile" Name="Mobile" Color="var(--chart-2)" ShowDots="true" />
            </LineChart>
        </ChartContainer>
        """;

    public const string Pie = """
        <ChartContainer Height="300">
            <PieChart Data="@pieData">
                <ChartTooltip />
                <Legend />
                <Pie DataKey="value" NameKey="name" />
            </PieChart>
        </ChartContainer>
        """;

    public const string Scatter = """
        <ChartContainer Height="300">
            <ScatterChart Data="@scatterData">
                <Grid Vertical="false" />
                <XAxis DataKey="x" />
                <YAxis DataKey="y" />
                <ChartTooltip />
                <Scatter DataKey="y" Name="样本" Color="var(--chart-1)" SymbolSize="10" />
            </ScatterChart>
        </ChartContainer>
        """;

    public const string Radar = """
        <ChartContainer Height="300">
            <RadarChart Data="@chartData">
                <RadarGrid />
                <XAxis DataKey="month" />
                <ChartTooltip />
                <Legend />
                <Radar DataKey="desktop" Name="Desktop" Color="var(--chart-1)" FillArea="true" />
                <Radar DataKey="mobile" Name="Mobile" Color="var(--chart-2)" FillArea="true" />
            </RadarChart>
        </ChartContainer>
        """;

    public const string RadialBar = """
        <ChartContainer Height="300">
            <RadialBarChart Data="@chartData">
                <PolarGrid />
                <XAxis DataKey="month" />
                <ChartTooltip />
                <Legend />
                <RadialBar DataKey="desktop" Name="Desktop" Color="var(--chart-1)" />
            </RadialBarChart>
        </ChartContainer>
        """;

    public const string Composed = """
        <ChartContainer Height="300">
            <ComposedChart Data="@chartData">
                <Grid Vertical="false" />
                <XAxis DataKey="month" />
                <YAxis />
                <ChartTooltip />
                <Legend />
                <Bar DataKey="desktop" Name="Desktop" Color="var(--chart-1)" />
                <Line DataKey="mobile" Name="Mobile" Color="var(--chart-2)" ShowDots="true" />
            </ComposedChart>
        </ChartContainer>
        """;

    public const string Candlestick = """
        <ChartContainer Height="300">
            <CandlestickChart Data="@ohlcData">
                <Grid Vertical="false" />
                <XAxis DataKey="date" />
                <YAxis />
                <ChartTooltip />
                <Candlestick DataKey="close" OpenKey="open" CloseKey="close" HighKey="high" LowKey="low" />
            </CandlestickChart>
        </ChartContainer>
        """;

    public const string Funnel = """
        <ChartContainer Height="300">
            <FunnelChart Data="@funnelData">
                <ChartTooltip />
                <Legend />
                <Funnel DataKey="value" NameKey="stage" ShowLabels="true" />
            </FunnelChart>
        </ChartContainer>
        """;

    public const string Gauge = """
        <ChartContainer Height="300">
            <GaugeChart Data="@gaugeData">
                <Gauge DataKey="value" Name="完成率" Color="var(--chart-1)" Min="0" Max="100" />
            </GaugeChart>
        </ChartContainer>
        """;

    public const string Heatmap = """
        <ChartContainer Height="300">
            <HeatmapChart Data="@heatData">
                <ChartTooltip />
                <VisualMap Min="0" Max="100" />
                <Heatmap DataKey="value" XKey="x" YKey="y" ValueKey="value" />
            </HeatmapChart>
        </ChartContainer>
        """;
}
