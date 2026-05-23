namespace NeoAdmin.Blazor.Pages.DemoUI;

internal static class OverlaysModalDemoSnippets
{
    public const string AlertDialog = """
        <AlertDialog @bind-Open="deleteAlertOpen">
            <AlertDialogTrigger AsChild>
                <Button Variant="ButtonVariant.Destructive">删除账户</Button>
            </AlertDialogTrigger>
            <AlertDialogContent>
                <AlertDialogHeader>
                    <AlertDialogTitle>确定要删除吗？</AlertDialogTitle>
                    <AlertDialogDescription>
                        此操作不可撤销，将永久删除你的账户及所有数据。
                    </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                    <AlertDialogCancel>取消</AlertDialogCancel>
                    <AlertDialogAction OnClick="OnDeleteConfirmed">继续</AlertDialogAction>
                </AlertDialogFooter>
            </AlertDialogContent>
        </AlertDialog>
        """;

    public const string Dialog = """
        <Dialog @bind-Open="editDialogOpen">
            <DialogTrigger AsChild>
                <Button>编辑资料</Button>
            </DialogTrigger>
            <DialogContent Class="sm:max-w-md">
                <DialogHeader>
                    <DialogTitle>编辑资料</DialogTitle>
                    <DialogDescription>修改显示名称后点击保存。</DialogDescription>
                </DialogHeader>
                <Input @bind-Value="displayName" Placeholder="显示名称" />
                <DialogFooter Class="mt-4">
                    <Button Variant="ButtonVariant.Outline" OnClick="CloseEditDialog">取消</Button>
                    <Button OnClick="SaveEditDialog">保存</Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
        """;

    public const string DialogService = """
        @inject DialogService DialogService

        <Button OnClick="RunConfirmAsync">确认对话框</Button>
        <Button OnClick="RunAlertAsync">提示对话框</Button>

        private async Task RunConfirmAsync()
        {
            DialogResult result = await DialogService.ConfirmAsync(new DialogOptions
            {
                Title = "确认提交",
                Message = "确定要保存当前更改吗？",
                Buttons = DialogButtons.OkCancel,
                ConfirmText = "保存",
                CancelText = "取消",
                Variant = DialogVariant.Default
            });

            dialogServiceMessage = result == DialogResult.Confirmed ? "用户已确认" : "用户已取消";
        }

        private async Task RunAlertAsync()
        {
            await DialogService.AlertAsync("操作已完成。", "提示", "好的");
        }
        """;

    public const string Drawer = """
        <Drawer @bind-Open="drawerOpen" Direction="DrawerDirection.Right">
            <DrawerTrigger AsChild>
                <Button Variant="ButtonVariant.Outline">打开抽屉</Button>
            </DrawerTrigger>
            <DrawerContent>
                <DrawerHeader>
                    <DrawerTitle>筛选条件</DrawerTitle>
                    <DrawerDescription>从右侧滑出的面板，适合放置筛选或详情。</DrawerDescription>
                </DrawerHeader>
                <div class="px-4 pb-4">
                    <Input @bind-Value="drawerKeyword" Placeholder="关键词…" />
                </div>
                <DrawerFooter>
                    <DrawerClose AsChild>
                        <Button Variant="ButtonVariant.Outline">关闭</Button>
                    </DrawerClose>
                    <Button OnClick="ApplyDrawerFilter">应用</Button>
                </DrawerFooter>
            </DrawerContent>
        </Drawer>
        """;

    public const string Sheet = """
        <Sheet @bind-Open="sheetOpen" Side="SheetSide.Right">
            <SheetTrigger AsChild>
                <Button>打开侧板</Button>
            </SheetTrigger>
            <SheetContent>
                <SheetHeader>
                    <SheetTitle>通知设置</SheetTitle>
                    <SheetDescription>管理邮件与站内消息偏好。</SheetDescription>
                </SheetHeader>
                <div class="grid gap-4 py-4">
                    <div class="flex items-center justify-between">
                        <Label>邮件通知</Label>
                        <Switch @bind-Checked="sheetEmailNotify" />
                    </div>
                </div>
                <SheetFooter>
                    <SheetClose AsChild>
                        <Button Variant="ButtonVariant.Outline">取消</Button>
                    </SheetClose>
                    <Button OnClick="SaveSheetSettings">保存</Button>
                </SheetFooter>
            </SheetContent>
        </Sheet>
        """;
}
