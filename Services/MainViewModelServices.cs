namespace HmiDesigner.Services;

public record MainViewModelServices(
    IDialogService DialogService,
    IFileService FileService
);
