namespace NeoAdmin.Blazor.Pages.DemoUI;

internal static class AdvancedDateTimeDemoSnippets
{
    public const string Calendar = """
        <Calendar @bind-SelectedDate="calendarDate"
                  Class="rounded-md border"
                  CaptionLayout="CalendarCaptionLayout.Dropdown" />

        <p class="text-sm text-muted-foreground">
            已选：@(calendarDate?.ToString("D") ?? "无")
        </p>
        """;

    public const string DateInput = """
        <Label For="date-input">日期</Label>
        <DatePicker Id="date-input"
                    Class="w-[280px]"
                    @bind-SelectedDate="dateInputValue"
                    Placeholder="选择日期" />

        <DatePicker Class="w-[280px]"
                    @bind-SelectedDate="dateInputBirth"
                    ShowIcon="false" />
        """;

    public const string DatePicker = """
        <Label>选择日期</Label>
        <DatePicker @bind-SelectedDate="datePickerValue"
                    Class="w-[280px]"
                    Placeholder="选择日期" />
        """;

    public const string DateRangePicker = """
        <DateRangePicker @bind-StartDate="rangeStart"
                         @bind-EndDate="rangeEnd"
                         Class="w-[320px]"
                         ShowPresets="true" />
        """;

    public const string TimeInput = """
        <Label For="time-input">时间（24 小时）</Label>
        <TimePicker Id="time-input"
                    Class="w-[240px]"
                    @bind-SelectedTime="timeInputValue"
                    Use12Hour="false" />

        <TimePicker Class="w-[240px]"
                    @bind-SelectedTime="timeInput12h"
                    Use12Hour="true" />
        """;

    public const string TimePicker = """
        <Label>选择时间</Label>
        <TimePicker @bind-SelectedTime="timePickerValue"
                    Class="w-[240px]"
                    Use12Hour="false"
                    ShowSeconds="false" />
        """;
}
