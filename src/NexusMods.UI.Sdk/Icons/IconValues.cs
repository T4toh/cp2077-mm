using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace NexusMods.UI.Sdk.Icons;

// https://www.figma.com/file/8pjtQeNggvVi7RWoLNGV80/%F0%9F%A7%B0-Nexus-Mods-Design-System?type=design&node-id=130-463

/*
    Important Notes!! - Sewer
    
    What not to do:
    
        - Paste Raw Coordinates from SVG in Figma
            - This will get you wrong icon sizes, as padding will be excluded.
            
    What to do:
    
        - Use Projektanker Icon if possible https://pictogrammers.com/library/mdi/icon/code-tags/
            - Projectanker Icons are the raw SVGs, so it's okay ^-^
        - Create a SimpleVectorIconImage based on the contents of an SVG 
            - This will give you the correct icon size, as padding etc. is preserved.
            - We can't use the raw SVGs as they don't support recolouring.
        - If you explicitly don't want recolouring for brand purposes, import an SVG like
            - AvaloniaSvg("avares://NexusMods.App.UI/Assets/Icons/disk_20px.svg");

    How to Import SVG:
    
        Exporting from Figma may give you an SVG like
        
        ```xml
        <svg width="25" height="25" viewBox="0 0 25 25" fill="none" xmlns="http://www.w3.org/2000/svg">
        <path fill-rule="evenodd" clip-rule="evenodd" d="M12.46 17.9912L18.1722 13.5441L19.445 12.5584L12.46 7.12561L5.47498 12.5584L6.74004 13.5441L12.46 17.9912Z" fill="#F4F4F5"/>
        </svg>
        ```
        
        You have to extract the `d` attribute from the `path` tag, and the `viewBox` attribute from the `svg` tag.
        
        Then create a `SimpleVectorIconImage` with the `d` attribute as the `pathData` and the `viewBox` attribute as the `viewBox`.
        
        ```csharp
        public static readonly IconValue Mods = new SimpleVectorIconImage(
            "M12.46 17.9912L18.1722 13.5441L19.445 12.5584L12.46 7.12561L5.47498 12.5584L6.74004 13.5441L12.46 17.9912Z",
            new Rect(0, 0, 25, 25)
        );
        ```
        
        If the SVG file has multiple paths, you can manually splice all the paths 'd' data together into a single path
        
*/

public static class IconValues
{

#region Action
    // https://pictogrammers.com/library/mdi/icon/code-tags/
    public static readonly IconValue Code = new ProjektankerIcon("mdi-code-tags");
    
    // https://pictogrammers.com/library/mdi/icon/check/
    public static readonly IconValue Check = new ProjektankerIcon("mdi-check");
    

    // https://pictogrammers.com/library/mdi/icon/check-bold/
    public static readonly IconValue CheckBold = new ProjektankerIcon("mdi-check-bold");
    
    // https://pictogrammers.com/library/mdi/icon/check-circle/
    public static readonly IconValue CheckCircle = new ProjektankerIcon("mdi-check-circle");
    
    // https://pictogrammers.com/library/mdi/icon/check-circle-outline/
    public static readonly IconValue CheckCircleOutline = new ProjektankerIcon("mdi-check-circle-outline");

    // https://pictogrammers.com/library/mdi/icon/delete-outline/
    public static readonly IconValue DeleteOutline = new ProjektankerIcon("mdi-delete-outline");

    // https://pictogrammers.com/library/mdi/icon/delete-forever/
    public static readonly IconValue DeleteForever = new ProjektankerIcon("mdi-delete-forever");

    // https://pictogrammers.com/library/mdi/icon/file-document/
    // This is mislabeled on Figma and some places as 'description'
    public static readonly IconValue Description = new ProjektankerIcon("mdi-file-document");
  
    // https://pictogrammers.com/library/mdi/icon/help-circle/
    public static readonly IconValue Help = new ProjektankerIcon("mdi-help-circle");

    // https://pictogrammers.com/library/mdi/icon/help-circle-outline/
    public static readonly IconValue HelpOutline = new ProjektankerIcon("mdi-help-circle-outline");

    // https://pictogrammers.com/library/mdi/icon/history/
    public static readonly IconValue History = new ProjektankerIcon("mdi-history");

    // https://pictogrammers.com/library/mdi/icon/home/
    public static readonly IconValue Home = new ProjektankerIcon("mdi-home");

    // https://pictogrammers.com/library/mdi/icon/lock/
    public static readonly IconValue Lock = new ProjektankerIcon("mdi-lock");

    // https://pictogrammers.com/library/mdi/icon/lock-outline/
    public static readonly IconValue LockOutline = new ProjektankerIcon("mdi-lock-outline");

    // https://pictogrammers.com/library/mdi/icon/open-in-new/
    public static readonly IconValue OpenInNew = new ProjektankerIcon("mdi-open-in-new");
    
    // https://pictogrammers.com/library/mdi/icon/format-list-bulleted/
    public static readonly IconValue FormatListBullet = new ProjektankerIcon("mdi-format-list-bulleted");
    
    // https://pictogrammers.com/library/mdi/icon/list-box-outline/
    public static readonly IconValue ListBoxOutline = new ProjektankerIcon("mdi-list-box-outline");

    // https://pictogrammers.com/library/mdi/icon/format-list-checkbox/
    public static readonly IconValue FormatListCheckbox = new ProjektankerIcon("mdi-format-list-checkbox");
    
    public static readonly IconValue FormatListNumbered = new ProjektankerIcon("mdi-format-list-numbered");
    
    // https://pictogrammers.com/library/mdi/icon/dots-grid/
    public static readonly IconValue DotsGrid = new ProjektankerIcon("mdi-dots-grid");
    
    // https://pictogrammers.com/library/mdi/icon/format-align-justify/
    public static readonly IconValue FormatAlignJustify = new ProjektankerIcon("mdi-format-align-justify");
    

    // https://pictogrammers.com/library/mdi/icon/playlist-plus/
    public static readonly IconValue PlaylistAdd = new ProjektankerIcon("mdi-playlist-plus");
    
    // https://pictogrammers.com/library/mdi/icon/playlist-remove/
    public static readonly IconValue PlaylistRemove = new ProjektankerIcon("mdi-playlist-remove");

    // https://pictogrammers.com/library/mdi/icon/tab/
    public static readonly IconValue Tab = new ProjektankerIcon("mdi-tab");

    // https://pictogrammers.com/library/mdi/icon/thumb-up/
    public static readonly IconValue ThumbUp = new ProjektankerIcon("mdi-thumb-up");
    
    // https://pictogrammers.com/library/mdi/icon/thumb-up-outline/
    public static readonly IconValue ThumbUpOutline = new ProjektankerIcon("mdi-thumb-up-outline");
    
    // https://pictogrammers.com/library/mdi/icon/thumbs-up-down/
    public static readonly IconValue ThumbsUpDown = new ProjektankerIcon("mdi-thumbs-up-down");
        
    // https://pictogrammers.com/library/mdi/icon/thumbs-up-down-outline/
    public static readonly IconValue ThumbsUpDownOutline = new ProjektankerIcon("mdi-thumbs-up-down-outline");
    
    // https://pictogrammers.com/library/mdi/icon/magnfiy/
    public static readonly IconValue Search = new ProjektankerIcon("mdi-magnify");

    // https://pictogrammers.com/library/mdi/icon/cog/
    public static readonly IconValue Cog = new ProjektankerIcon("mdi-cog");
    
    // https://pictogrammers.com/library/mdi/icon/cog-outline/
    public static readonly IconValue CogOutline = new ProjektankerIcon("mdi-cog-outline");

    // https://pictogrammers.com/library/mdi/icon/eye/
    public static readonly IconValue Visibility = new ProjektankerIcon("mdi-eye");

    // https://pictogrammers.com/library/mdi/icon/eye/
    public static readonly IconValue VisibilityOff = new ProjektankerIcon("mdi-eye-off");
    
    // https://pictogrammers.com/library/mdi/icon/view-carousel/
    public static readonly IconValue ViewCarousel = new ProjektankerIcon("mdi-view-carousel");

    // https://pictogrammers.com/library/mdi/icon/sort/
    public static readonly IconValue Sort = new ProjektankerIcon("mdi-sort");

    // https://pictogrammers.com/library/mdi/icon/sort-ascending/
    public static readonly IconValue SortAscending = new ProjektankerIcon("mdi-sort-ascending");

    // https://pictogrammers.com/library/mdi/icon/sort-descending/
    public static readonly IconValue SortDescending = new ProjektankerIcon("mdi-sort-descending");

    // https://pictogrammers.com/library/mdi/icon/account-cog/
    public static readonly IconValue AccountCog = new ProjektankerIcon("mdi-account-cog");
    
    // https://pictogrammers.com/library/mdi/icon/logout/
    public static readonly IconValue Logout = new ProjektankerIcon("mdi-logout");
    
    // https://pictogrammers.com/library/mdi/icon/link/
    public static readonly IconValue Link = new ProjektankerIcon("mdi-link");
#endregion

#region Alert

    // https://pictogrammers.com/library/mdi/icon/alert-circle/
    public static readonly IconValue Error = new ProjektankerIcon("mdi-alert-circle");

    // https://pictogrammers.com/library/mdi/icon/alert/
    public static readonly IconValue Warning = new ProjektankerIcon("mdi-alert");

    // https://pictogrammers.com/library/mdi/icon/alert-outline/
    public static readonly IconValue WarningAmber = new ProjektankerIcon("mdi-alert-outline");

    // https://pictogrammers.com/library/mdi/icon/bell/
    public static readonly IconValue NotificationImportant = new ProjektankerIcon("mdi-bell");
    
    // https://pictogrammers.com/library/mdi/icon/information-outline/
    public static readonly IconValue Info = new ProjektankerIcon("mdi-information-outline");

    // https://pictogrammers.com/library/mdi/icon/information/
    public static readonly IconValue InfoFilled = new ProjektankerIcon("mdi-information");
    
#endregion

#region AV

    // https://pictogrammers.com/library/mdi/icon/pause-circle/
    public static readonly IconValue PauseCircleFilled = new ProjektankerIcon("mdi-pause-circle");

    // https://pictogrammers.com/library/mdi/icon/pause-circle-outline/
    public static readonly IconValue PauseCircleOutline = new ProjektankerIcon("mdi-pause-circle-outline");

    // https://pictogrammers.com/library/mdi/icon/play/
    public static readonly IconValue PlayArrow = new ProjektankerIcon("mdi-play");

    // https://pictogrammers.com/library/mdi/icon/play-circle/
    public static readonly IconValue PlayCircleFilled = new ProjektankerIcon("mdi-play-circle");

    // https://pictogrammers.com/library/mdi/icon/play-circle-outline/
    public static readonly IconValue PlayCircleOutline = new ProjektankerIcon("mdi-play-circle-outline");
    
    
    // https://pictogrammers.com/library/mdi/icon/pause/
    public static readonly IconValue Pause = new ProjektankerIcon("mdi-pause");

#endregion

#region Communication

    // https://pictogrammers.com/library/mdi/icon/notification-clear-all/
    public static readonly IconValue ClearAll = new ProjektankerIcon("mdi-notification-clear-all");

    // https://pictogrammers.com/library/mdi/icon/tooltip-question/
    public static readonly IconValue LiveHelp = new ProjektankerIcon("mdi-tooltip-question");
    
    // https://pictogrammers.com/library/mdi/icon/party-popper/
    public static readonly IconValue PartyPopper = new ProjektankerIcon("mdi-party-popper");
    
    // https://pictogrammers.com/library/mdi/icon/broadcast/
    public static readonly IconValue Broadcast = new ProjektankerIcon("mdi-broadcast");

#endregion

#region Content

    // https://pictogrammers.com/library/mdi/icon/plus/
    public static readonly IconValue Add = new ProjektankerIcon("mdi-plus");

    // https://pictogrammers.com/library/mdi/icon/plus-circle/
    public static readonly IconValue AddCircle = new ProjektankerIcon("mdi-plus-circle");

    // https://pictogrammers.com/library/mdi/icon/plus-circle-outline/
    public static readonly IconValue AddCircleOutline = new ProjektankerIcon("mdi-plus-circle-outline");

    // https://pictogrammers.com/library/mdi/icon/block-helper/
    public static readonly IconValue Block = new ProjektankerIcon("mdi-block-helper");

    // https://pictogrammers.com/library/mdi/icon/content-copy/
    public static readonly IconValue Copy = new ProjektankerIcon("mdi-content-copy");

    // https://pictogrammers.com/library/mdi/icon/content-paste/
    public static readonly IconValue Paste = new ProjektankerIcon("mdi-content-paste");

    // https://pictogrammers.com/library/mdi/icon/redo/
    public static readonly IconValue Redo = new ProjektankerIcon("mdi-redo");

    // https://pictogrammers.com/library/mdi/icon/minus/
    public static readonly IconValue Remove = new ProjektankerIcon("mdi-minus");

    // https://pictogrammers.com/library/mdi/icon/minus-circle-outline/
    public static readonly IconValue RemoveCircleOutline = new ProjektankerIcon("mdi-minus-circle-outline");

    // https://pictogrammers.com/library/mdi/icon/content-save/
    public static readonly IconValue Save = new ProjektankerIcon("mdi-content-save");

    // https://pictogrammers.com/library/mdi/icon/undo/
    public static readonly IconValue Undo = new ProjektankerIcon("mdi-undo");
    
    // https://pictogrammers.com/library/mdi/icon/tray-arrow-down/
    public static readonly IconValue TrayArrowDown = new ProjektankerIcon("mdi-tray-arrow-down");

#endregion

#region Editor

    // https://pictogrammers.com/library/mdi/icon/poll
    public static readonly IconValue BarChart = new ProjektankerIcon("mdi-poll");

    // https://pictogrammers.com/library/mdi/icon/drag-horizontal-variant/
    public static readonly IconValue DragHandleHorizontal = new ProjektankerIcon("mdi-drag-horizontal-variant");

    // https://pictogrammers.com/library/mdi/icon/drag-vertical-variant/
    public static readonly IconValue DragHandleVertical = new ProjektankerIcon("mdi-drag-vertical-variant");

    // https://pictogrammers.com/library/mdi/icon/file-outline/
    public static readonly IconValue File = new ProjektankerIcon("mdi-file-outline");

#endregion

#region File

    // https://pictogrammers.com/library/mdi/icon/download/
    public static readonly IconValue Download = new ProjektankerIcon("mdi-download");

    // https://pictogrammers.com/library/mdi/icon/check-underline/
    public static readonly IconValue DownloadDone = new ProjektankerIcon("mdi-check-underline");

    // https://pictogrammers.com/library/mdi/icon/folder-outline/
    public static readonly IconValue Folder = new ProjektankerIcon("mdi-folder-outline");
    
    // https://pictogrammers.com/library/mdi/icon/folder-eye-outline/
    public static readonly IconValue FolderEyeOutline = new ProjektankerIcon("mdi-folder-eye-outline");

    // https://pictogrammers.com/library/mdi/icon/check-underline/
    public static readonly IconValue FolderOpen = new ProjektankerIcon("mdi-folder-open-outline");
    
    // https://pictogrammers.com/library/mdi/icon/folder-edit-outline/
    public static readonly IconValue FolderEditOutline = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M160-240v-480 480Zm0 80q-33 0-56.5-23.5T80-240v-480q0-33 23.5-56.5T160-800h240l80 80h320q33 0 56.5 23.5T880-640v132q-19-8-39.5-10.5t-40.5.5v-122H447l-80-80H160v480h324l-4 4v76H160Zm400 80v-123l221-220q9-9 20-13t22-4q12 0 23 4.5t20 13.5l37 37q8 9 12.5 20t4.5 22q0 11-4 22.5T903-300L683-80H560Zm300-263-37-37 37 37ZM620-140h38l121-122-18-19-19-18-122 121v38Zm141-141-19-18 37 37-18-19Z",
        new Rect(0, -960, 960, 960)
    ));

    // https://pictogrammers.com/library/mdi/icon/file-edit/
    public static readonly IconValue FileEdit = new ProjektankerIcon("mdi-file-edit");

    // https://pictogrammers.com/library/mdi/icon/video-outline/
    public static readonly IconValue Video = new ProjektankerIcon("mdi-video-outline");

    // https://pictogrammers.com/library/mdi/icon/music-note/
    public static readonly IconValue MusicNote = new ProjektankerIcon("mdi-music-note");

    // https://pictogrammers.com/library/mdi/icon/file-document-outline/
    public static readonly IconValue FileDocumentOutline = new ProjektankerIcon("mdi-file-document-outline");
    
    // https://pictogrammers.com/library/mdi/icon/folder-upload-outline/
    public static readonly IconValue FolderUploadOutline = new ProjektankerIcon("mdi-folder-upload-outline");
    
    // https://pictogrammers.com/library/mdi/icon/update/
    public static readonly IconValue Update = new ProjektankerIcon("mdi-update");
    
    // https://pictogrammers.com/library/mdi/icon/upload/
    public static readonly IconValue Upload = new ProjektankerIcon("mdi-upload");
    
    // https://pictogrammers.com/library/mdi/icon/export-variant/
    public static readonly IconValue ExportVariant = new ProjektankerIcon("mdi-export-variant");
    
    // https://pictogrammers.com/library/mdi/icon/pencil/
    public static readonly IconValue Pencil = new ProjektankerIcon("mdi-pencil");
    
    // https://pictogrammers.com/library/mdi/icon/cloud-upload-outline/
    public static readonly IconValue CloudUpload = new ProjektankerIcon("mdi-cloud-upload-outline");

#endregion

#region Hardware

    // https://pictogrammers.com/library/mdi/icon/monitor/
    public static readonly IconValue Desktop = new ProjektankerIcon("mdi-monitor");

    // https://pictogrammers.com/library/mdi/icon/gamepad-square/
    public static readonly IconValue Game = new ProjektankerIcon("mdi-gamepad-square");
    
    // https://pictogrammers.com/library/mdi/icon/database/
    public static readonly IconValue Database = new ProjektankerIcon("mdi-database");
    
    // https://pictogrammers.com/library/mdi/icon/package-variant-closed/
    public static readonly IconValue Package = new ProjektankerIcon("mdi-package-variant-closed");

#endregion

#region Image

    // https://pictogrammers.com/library/mdi/icon/image/
    public static readonly IconValue Image = new ProjektankerIcon("mdi-image");

    // https://pictogrammers.com/library/mdi/icon/image-outline/
    public static readonly IconValue ImageOutline = new ProjektankerIcon("mdi-image-outline");

    // https://pictogrammers.com/library/mdi/icon/tune/
    public static readonly IconValue Tune = new ProjektankerIcon("mdi-tune");
    
    // https://pictogrammers.com/library/mdi/icon/palette/
    public static readonly IconValue ColorLens = new ProjektankerIcon("mdi-palette");
    
    // https://pictogrammers.com/library/mdi/icon/camera-plus/
    public static readonly IconValue CameraPlus = new ProjektankerIcon("mdi-camera-plus");

#endregion

#region Navigation

    // https://pictogrammers.com/library/mdi/icon/arrow-left/
    public static readonly IconValue ArrowBack = new ProjektankerIcon("mdi-arrow-left");

    // https://pictogrammers.com/library/mdi/icon/arrow-right/
    public static readonly IconValue ArrowForward = new ProjektankerIcon("mdi-arrow-right");
    
    // https://pictogrammers.com/library/mdi/icon/arrow-up/
    public static readonly IconValue ArrowUp = new ProjektankerIcon("mdi-arrow-up");
    
    // https://pictogrammers.com/library/mdi/icon/arrow-down/
    public static readonly IconValue ArrowDown = new ProjektankerIcon("mdi-arrow-down");

    // https://pictogrammers.com/library/mdi/icon/menu-down/
    public static readonly IconValue ArrowDropDown = new ProjektankerIcon("mdi-menu-down");

    // https://pictogrammers.com/library/mdi/icon/menu-up/
    public static readonly IconValue ArrowDropUp = new ProjektankerIcon("mdi-menu-up");
    
    // https://pictogrammers.com/library/mdi/icon/arrow-up-thick/
    public static readonly IconValue ArrowUpThick = new ProjektankerIcon("mdi-arrow-up-thick");
    
    // https://pictogrammers.com/library/mdi/icon/arrow-down-thick"/
    public static readonly IconValue ArrowDownThick = new ProjektankerIcon("mdi-arrow-down-thick");
    
    // https://pictogrammers.com/library/mdi/icon/chevron-left/
    public static readonly IconValue ChevronLeft = new ProjektankerIcon("mdi-chevron-left");

    // https://pictogrammers.com/library/mdi/icon/chevron-right/
    public static readonly IconValue ChevronRight = new ProjektankerIcon("mdi-chevron-right");
    
    // https://pictogrammers.com/library/mdi/icon/chevron-down/
    public static readonly IconValue ChevronDown = new ProjektankerIcon("mdi-chevron-down");
    
    // https://pictogrammers.com/library/mdi/icon/chevron-up/
    public static readonly IconValue ChevronUp = new ProjektankerIcon("mdi-chevron-up");

    // https://pictogrammers.com/library/mdi/icon/close/
    public static readonly IconValue Close = new ProjektankerIcon("mdi-close");
    
    // https://pictogrammers.com/library/mdi/icon/window-minimize/
    public static readonly IconValue WindowMinimize = new ProjektankerIcon("mdi-window-minimize");
    
    // https://pictogrammers.com/library/mdi/icon/window-maximize/
    public static readonly IconValue WindowMaximize = new ProjektankerIcon("mdi-window-maximize");
    
    // https://pictogrammers.com/library/mdi/icon/window-restore/
    public static readonly IconValue WindowRestore = new ProjektankerIcon("mdi-window-restore");
    
    // https://pictogrammers.com/library/mdi/icon/refresh/
    public static readonly IconValue Refresh = new ProjektankerIcon("mdi-refresh");

    // https://pictogrammers.com/library/mdi/icon/swap-vertical"/
    public static readonly IconValue Swap = new ProjektankerIcon("mdi-swap-vertical");
    
    // https://pictogrammers.com/library/mdi/icon/dots-vertical/
    public static readonly IconValue MoreVertical = new ProjektankerIcon("mdi-dots-vertical");
    

#endregion
    
#region Notification
    
    // https://pictogrammers.com/library/mdi/icon/sync/
    public static readonly IconValue Sync = new ProjektankerIcon("mdi-sync");

    // https://pictogrammers.com/library/mdi/icon/lightbulb/
    public static readonly IconValue Lightbulb = new ProjektankerIcon("mdi-lightbulb");
    
    // https://pictogrammers.com/library/mdi/icon/trophy/
    public static readonly IconValue Trophy = new ProjektankerIcon("mdi-trophy");
    
    // https://pictogrammers.com/library/mdi/icon/trophy-outline/
    public static readonly IconValue TrophyOutline = new ProjektankerIcon("mdi-trophy-outline");
    
    // https://pictogrammers.com/library/mdi/icon/notification-clear-all/
    public static readonly IconValue NotificationClearAll = new ProjektankerIcon("mdi-notification-clear-all");
    
    
#endregion

#region Social

    // https://pictogrammers.com/library/mdi/icon/school
    public static readonly IconValue School = new ProjektankerIcon("mdi-school");

    // https://pictogrammers.com/library/mdi/icon/account
    public static readonly IconValue Account = new ProjektankerIcon("mdi-account");
    
#endregion

#region Toggle
    
    // https://pictogrammers.com/library/mdi/icon/checkbox-marked/
    public static readonly IconValue CheckBox = new ProjektankerIcon("mdi-checkbox-marked");

    // https://pictogrammers.com/library/mdi/icon/star/
    public static readonly IconValue Star = new ProjektankerIcon("mdi-star");

#endregion

#region Custom Icons
    
    // https://pictogrammers.com/library/mdi/icon/alert-octagon/
    public static readonly IconValue Alert = new ProjektankerIcon("mdi-alert-octagon");

    // From Design System "Custom Icons" section on Figma
    public static readonly IconValue Mods = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M12.46 17.9912L18.1722 13.5441L19.445 12.5584L12.46 7.12561L5.47498 12.5584L6.74004 13.5441L12.46 17.9912Z",
        new Rect(0, 0, 25, 25)
        ));

    // From Design System "Custom Icons" section on Figma
    public static readonly IconValue Collections = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M12.1979 15.4946L6.68644 11.2096L5.47498 12.1518L12.2053 17.3866L18.9357 12.1518L17.7167 11.2021L12.1979 15.4946ZM12.1979 19.2336L6.68644 14.9486L5.47498 15.8908L12.2053 21.1255L18.9357 15.8908L17.7167 14.9411L12.1979 19.2336ZM12.2053 13.5951L17.7093 9.31006L18.9357 8.36033L12.2053 3.12561L5.47498 8.36033L6.69392 9.31006L12.2053 13.5951Z",
        new Rect(0, 0, 25, 25)
    ));
    
    // From Design System "Custom Icons" section on Figma
    public static readonly IconValue ListFilled = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M9.39429 18.8744H20.3943V16.1994H9.39429V18.8744ZM4.39429 9.54939H7.39429V6.87439H4.39429V9.54939ZM4.39429 14.2244H7.39429V11.5494H4.39429V14.2244ZM4.39429 18.8744H7.39429V16.1994H4.39429V18.8744ZM9.39429 14.2244H20.3943V11.5494H9.39429V14.2244ZM9.39429 9.54939H20.3943V6.87439H9.39429V9.54939ZM4.39429 20.8744C3.84429 20.8744 3.37345 20.6786 2.98179 20.2869C2.59012 19.8952 2.39429 19.4244 2.39429 18.8744V6.87439C2.39429 6.32439 2.59012 5.85356 2.98179 5.46189C3.37345 5.07022 3.84429 4.87439 4.39429 4.87439H20.3943C20.9443 4.87439 21.4151 5.07022 21.8068 5.46189C22.1985 5.85356 22.3943 6.32439 22.3943 6.87439V18.8744C22.3943 19.4244 22.1985 19.8952 21.8068 20.2869C21.4151 20.6786 20.9443 20.8744 20.3943 20.8744H4.39429Z",
        new Rect(0, 0, 25, 25)
    ));
    
    // https://pictogrammers.com/library/mdi/icon/progress-download/
    public static readonly IconValue Downloading = new ProjektankerIcon("mdi-progress-download");

    // From Design System "Custom Icons" section on Figma
    public static readonly IconValue ModLibrary = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M18.3721 4.87439H6.40772C6.40772 4.87439 6.1358 2.87439 8.03922 2.87439H17.2844C18.644 2.87439 18.3721 4.87439 18.3721 4.87439ZM22.3943 20.5411V11.2077C22.3943 9.92439 21.4943 8.87439 20.3943 8.87439H4.39429C3.29429 8.87439 2.39429 9.92439 2.39429 11.2077V20.5411C2.39429 21.8244 3.29429 22.8744 4.39429 22.8744H20.3943C21.4943 22.8744 22.3943 21.8244 22.3943 20.5411ZM4.41219 7.87439H20.3647C20.3647 7.87439 20.7272 5.87439 18.9145 5.87439H6.58753C4.04963 5.87439 4.41219 7.87439 4.41219 7.87439ZM12.3943 11.8744L18.3943 15.8805L12.3943 19.8744L6.39429 15.8805L12.3943 11.8744Z",
        new Rect(0, 0, 25, 25)
    ));

    // From Design System "Custom Icons" section on Figma
    public static readonly IconValue Discord = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M19.4058 5.38929C18.1311 4.80439 16.7641 4.37346 15.3349 4.12665C15.3089 4.12188 15.2829 4.13379 15.2695 4.15759C15.0937 4.47027 14.8989 4.87819 14.7626 5.19881C13.2253 4.96867 11.696 4.96867 10.1902 5.19881C10.0538 4.87106 9.85205 4.47027 9.67546 4.15759C9.66205 4.13458 9.63605 4.12268 9.61002 4.12665C8.18157 4.37267 6.81461 4.8036 5.53909 5.38929C5.52805 5.39405 5.51858 5.40199 5.5123 5.4123C2.91947 9.28593 2.20918 13.0644 2.55763 16.7959C2.5592 16.8142 2.56945 16.8317 2.58364 16.8428C4.29432 18.099 5.9514 18.8617 7.57771 19.3672C7.60374 19.3752 7.63131 19.3657 7.64788 19.3442C8.03258 18.8189 8.37551 18.2649 8.66954 17.6824C8.68689 17.6483 8.67033 17.6078 8.63486 17.5943C8.09092 17.388 7.57298 17.1364 7.07475 16.8507C7.03534 16.8277 7.03219 16.7713 7.06844 16.7443C7.17329 16.6658 7.27816 16.584 7.37827 16.5015C7.39638 16.4864 7.42162 16.4832 7.44292 16.4928C10.716 17.9871 14.2596 17.9871 17.4941 16.4928C17.5154 16.4824 17.5406 16.4856 17.5595 16.5007C17.6597 16.5832 17.7645 16.6658 17.8702 16.7443C17.9064 16.7713 17.904 16.8277 17.8646 16.8507C17.3664 17.1419 16.8485 17.388 16.3037 17.5935C16.2683 17.607 16.2525 17.6483 16.2698 17.6824C16.5702 18.2641 16.9131 18.818 17.2907 19.3434C17.3065 19.3657 17.3349 19.3752 17.3609 19.3672C18.9951 18.8617 20.6522 18.099 22.3628 16.8428C22.3778 16.8317 22.3873 16.815 22.3889 16.7967C22.8059 12.4826 21.6904 8.73517 19.4318 5.41309C19.4263 5.40199 19.4169 5.39405 19.4058 5.38929ZM9.15833 14.5238C8.17289 14.5238 7.36092 13.6191 7.36092 12.508C7.36092 11.3969 8.15715 10.4922 9.15833 10.4922C10.1674 10.4922 10.9715 11.4049 10.9557 12.508C10.9557 13.6191 10.1595 14.5238 9.15833 14.5238ZM15.8039 14.5238C14.8185 14.5238 14.0066 13.6191 14.0066 12.508C14.0066 11.3969 14.8028 10.4922 15.8039 10.4922C16.813 10.4922 17.6171 11.4049 17.6013 12.508C17.6013 13.6191 16.813 14.5238 15.8039 14.5238Z",
        new Rect(0, 0, 25, 25)
    ));

    // From Design System "Custom Icons" section on Figma
    public static readonly IconValue Forum = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M15.4751 4.12561V11.1256H5.6451L4.4751 12.2956V4.12561H15.4751ZM16.4751 2.12561H3.4751C2.9251 2.12561 2.4751 2.57561 2.4751 3.12561V17.1256L6.4751 13.1256H16.4751C17.0251 13.1256 17.4751 12.6756 17.4751 12.1256V3.12561C17.4751 2.57561 17.0251 2.12561 16.4751 2.12561ZM21.4751 6.12561H19.4751V15.1256H6.4751V17.1256C6.4751 17.6756 6.9251 18.1256 7.4751 18.1256H18.4751L22.4751 22.1256V7.12561C22.4751 6.57561 22.0251 6.12561 21.4751 6.12561Z",
        new Rect(0, 0, 25, 25)
    ));
    
    // Custom Icon from Figma. The source of this icon is currently unknown.
    // Need to ask. - Sewer
    public static readonly IconValue HardDrive = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M4 17H20V11H4V17ZM17 15.5C17.4167 15.5 17.7708 15.3542 18.0625 15.0625C18.3542 14.7708 18.5 14.4167 18.5 14C18.5 13.5833 18.3542 13.2292 18.0625 12.9375C17.7708 12.6458 17.4167 12.5 17 12.5C16.5833 12.5 16.2292 12.6458 15.9375 12.9375C15.6458 13.2292 15.5 13.5833 15.5 14C15.5 14.4167 15.6458 14.7708 15.9375 15.0625C16.2292 15.3542 16.5833 15.5 17 15.5ZM22 9H19.175L17.175 7H6.825L4.825 9H2L5.425 5.575C5.60833 5.39167 5.82083 5.25 6.0625 5.15C6.30417 5.05 6.55833 5 6.825 5H17.175C17.4417 5 17.6958 5.05 17.9375 5.15C18.1792 5.25 18.3917 5.39167 18.575 5.575L22 9ZM4 19C3.45 19 2.97917 18.8042 2.5875 18.4125C2.19583 18.0208 2 17.55 2 17V9H22V17C22 17.55 21.8042 18.0208 21.4125 18.4125C21.0208 18.8042 20.55 19 20 19H4Z",
        new Rect(0, 0, 24, 24)
    ));

    // The Black and White Nexus 'Developer' Logo.
    // This is the variation of the Nexus logo used in the App, and on the Discord.
    public static readonly IconValue Nexus = new SimpleVectorIcon(new SimpleVectorIconImage(
            "M17.0963 0C16.5785 0 16.1734 0.142228 16.0273 0.193129L16.0234 0.19485C15.4195 0.394796 14.831 0.701523 14.2267 1.09167C14.1094 1.06767 13.9924 1.04077 13.875 1.0207C12.9854 0.868622 12.0683 0.82619 11.1644 0.895103C10.3016 0.960718 9.44838 1.12793 8.62698 1.3919C8.37395 1.47312 8.12573 1.57222 7.87861 1.67192C7.46212 1.52838 7.0229 1.38287 6.51125 1.30201C6.11078 1.23879 5.7113 1.20652 5.32056 1.20652H5.30595C5.11795 1.20696 4.92998 1.2157 4.74544 1.23104C4.08978 1.26228 3.56161 1.49197 3.18852 1.76999C3.0329 1.88354 2.88888 2.0139 2.7591 2.15969L2.77328 2.14377L2.58157 2.34594C2.35009 2.58848 2.12336 2.8448 1.90627 3.10899C1.41623 3.70544 1.01778 4.27182 0.694091 4.83812C0.550546 5.0895 0.354764 5.45228 0.202341 5.89968C0.119442 6.14292 0.0604333 6.38316 0.0269614 6.62445C-0.0676257 7.31048 0.112619 7.8627 0.169242 8.03657L0.174831 8.05376L0.173539 8.04859C0.427551 8.84109 0.800046 9.47199 1.12566 9.96569C1.10728 10.0674 1.08653 10.1685 1.07107 10.2707C0.934139 11.1742 0.909057 12.0941 0.996705 13.005C1.0801 13.8704 1.26613 14.7249 1.54949 15.5462C1.57071 15.6077 1.59464 15.6683 1.61698 15.7294C1.43192 16.2205 1.21232 16.8611 1.10632 17.6083C1.0517 17.993 1.0261 18.3773 1.03023 18.7533C1.032 18.9189 1.04027 19.0846 1.05387 19.2492C1.08212 19.8435 1.27235 20.3341 1.51511 20.6975C1.64502 20.8952 1.80235 21.0781 1.98493 21.2399L1.96817 21.2244L2.16891 21.4154C2.41446 21.6498 2.67287 21.8785 2.93834 22.0967C3.54143 22.5921 4.11349 22.9938 4.68397 23.3178C4.9395 23.4629 5.30845 23.661 5.76418 23.8112C6.01684 23.8945 6.26643 23.9514 6.51642 23.9807H6.51769C6.6291 23.9935 6.74121 24 6.85384 24C7.36696 24 7.76609 23.8615 7.91729 23.8086L7.91987 23.8077L7.93104 23.8038C8.58147 23.5879 9.2177 23.2465 9.88128 22.8029C9.90033 22.8066 9.91941 22.8113 9.93845 22.8149C10.8289 22.985 11.7482 23.045 12.6564 22.9921C13.5298 22.9416 14.3956 22.787 15.2308 22.5328C15.4407 22.4688 15.6474 22.391 15.8541 22.3143C16.3191 22.487 16.8381 22.6562 17.4269 22.7539C17.8486 22.8239 18.2697 22.8597 18.6808 22.8597H18.6958C18.8827 22.859 19.0708 22.8502 19.2563 22.8347C19.9079 22.8034 20.4335 22.5768 20.8059 22.3014C20.9633 22.1872 21.1106 22.0549 21.2435 21.9048L21.228 21.922L21.3905 21.7508L21.3884 21.7525C21.8608 21.2594 22.3028 20.7252 22.7042 20.1623C23.0595 19.6637 23.4951 19.0271 23.7814 18.2177C23.8651 17.9811 23.9264 17.7477 23.9636 17.5144C24.0758 16.811 23.8974 16.2392 23.8471 16.0769C23.6967 15.5921 23.4868 15.1135 23.2251 14.6519C23.1193 14.4654 22.9884 14.2689 22.8597 14.0729C23.1278 12.706 23.1488 11.2938 22.895 9.92087C22.782 9.3096 22.5947 8.71439 22.38 8.1298C22.4807 7.84962 22.5793 7.56887 22.6577 7.29019C22.7989 6.78756 22.8833 6.25131 22.9113 5.69397C22.924 5.44537 22.9223 5.19722 22.909 4.95199C22.9026 4.18017 22.6518 3.56263 22.3158 3.13512C22.2122 3.00099 22.0949 2.8751 21.9651 2.76004L21.9823 2.77596L21.782 2.58498C21.5361 2.35027 21.2778 2.1218 21.0119 1.90333C20.409 1.40802 19.8378 1.00654 19.2671 0.682618C19.0112 0.537143 18.6423 0.339502 18.1864 0.189258C17.934 0.106095 17.6841 0.0490344 17.4338 0.019786C17.3215 0.00666226 17.2088 0 17.0963 0ZM17.0963 1.32136C17.1581 1.32136 17.221 1.32514 17.2807 1.33212C17.4366 1.35032 17.5945 1.38545 17.7734 1.44438C18.1025 1.55286 18.3878 1.70257 18.6146 1.8315C19.1072 2.11109 19.6171 2.46681 20.1741 2.92447C20.4145 3.12194 20.6494 3.33035 20.8713 3.54214L21.0811 3.74172L21.0897 3.74946C21.1587 3.81066 21.22 3.87606 21.2732 3.94517L21.2754 3.94818L21.2775 3.95076C21.4456 4.16429 21.5869 4.45698 21.59 4.96716V4.98437L21.5908 5.00114C21.6026 5.20572 21.6033 5.4161 21.5925 5.62698C21.569 6.09414 21.4985 6.53451 21.3867 6.93243C21.2991 7.24343 21.1942 7.561 21.0742 7.87743L20.9834 8.11658L21.0776 8.35487C21.3087 8.93945 21.4826 9.54561 21.5964 10.1614C21.8348 11.451 21.8084 12.788 21.5229 14.0679L21.4602 14.3492L21.6235 14.5866C21.8015 14.8452 21.9495 15.08 22.0766 15.3041C22.2942 15.6881 22.4657 16.0808 22.5859 16.4684C22.636 16.6298 22.722 16.9138 22.6594 17.3063C22.6359 17.4537 22.5975 17.6053 22.5369 17.7765C22.3192 18.3922 21.9696 18.9173 21.629 19.3951C21.2644 19.9065 20.8625 20.3924 20.4353 20.8381L20.4345 20.8394L20.263 21.0197L20.2553 21.0283C20.1867 21.1057 20.1111 21.1738 20.0292 21.233L20.0257 21.2356L20.0227 21.2377C19.8344 21.3774 19.5849 21.4976 19.1909 21.516L19.1776 21.5165L19.1647 21.5178C19.0115 21.5311 18.8521 21.5379 18.6923 21.5384H18.6807C18.3468 21.5384 17.9965 21.5089 17.643 21.4502C17.0879 21.3581 16.5654 21.1838 16.0947 20.9994L15.8519 20.9048L15.6098 21.0007C15.3587 21.1006 15.1027 21.1906 14.8464 21.2687C14.1118 21.4924 13.3486 21.6286 12.5798 21.673C11.7816 21.7195 10.9688 21.6662 10.1868 21.5169C10.0917 21.4987 9.99632 21.479 9.90185 21.458L9.61601 21.3947L9.37744 21.5637C8.69248 22.0485 8.10535 22.3543 7.51189 22.5509L7.51019 22.5518L7.49084 22.5582L7.48783 22.5591C7.34412 22.6095 7.14075 22.6787 6.8538 22.6787C6.79239 22.6787 6.72924 22.6749 6.66854 22.668C6.51306 22.6497 6.35566 22.6149 6.17722 22.5561C5.84848 22.4478 5.56268 22.2975 5.33557 22.1686C4.84286 21.8887 4.33369 21.5333 3.7765 21.0756C3.53583 20.8778 3.30095 20.6701 3.0793 20.4584L2.8691 20.2584L2.86051 20.2507C2.76418 20.1652 2.68311 20.0711 2.61725 19.9706L2.61555 19.9676L2.61385 19.965C2.49122 19.782 2.38937 19.5441 2.37271 19.1847L2.37228 19.1718L2.37101 19.1584C2.35902 19.0189 2.3523 18.8785 2.35081 18.7391C2.34744 18.4336 2.36848 18.1148 2.414 17.7941C2.50495 17.153 2.71114 16.5527 2.89027 16.0856C2.90463 16.0481 2.91964 16.0098 2.93455 15.972L3.02998 15.7303L2.93498 15.4886C2.88628 15.3643 2.8403 15.2394 2.79734 15.1148C2.54813 14.3926 2.38456 13.6396 2.31117 12.8781C2.23394 12.0756 2.25595 11.2645 2.37651 10.469C2.40176 10.3021 2.43202 10.1338 2.46634 9.96658L2.52309 9.69044L2.36275 9.45816C2.04797 9.00219 1.66293 8.36927 1.43084 7.64515L1.42482 7.62709C1.3679 7.45229 1.28227 7.18787 1.33498 6.80554C1.35618 6.65293 1.39265 6.49974 1.4519 6.32594C1.56278 6.00048 1.71262 5.71801 1.84049 5.49407C2.12074 5.0038 2.47377 4.49896 2.92629 3.94817C3.12323 3.70849 3.32893 3.4759 3.53711 3.25781L3.53795 3.25697L3.73783 3.04664L3.74471 3.0389C3.81369 2.96141 3.88894 2.89335 3.96866 2.83545L3.97554 2.83028C4.16415 2.68937 4.41385 2.56863 4.81031 2.55028L4.82364 2.54942L4.83653 2.54855C4.98998 2.5353 5.14858 2.52833 5.30679 2.52791H5.32054C5.63812 2.52791 5.97015 2.55414 6.30533 2.60706C6.79887 2.68506 7.2626 2.82686 7.68602 2.98299L7.93275 3.07418L8.17433 2.97052C8.45411 2.85054 8.74206 2.74267 9.03059 2.65007C9.75276 2.41798 10.505 2.27082 11.2645 2.21306C12.0596 2.15244 12.8719 2.19011 13.6528 2.3236C13.8345 2.35467 14.0167 2.39104 14.1978 2.43285L14.4807 2.49823L14.7197 2.33392C15.337 1.91006 15.8819 1.63332 16.4382 1.44913L16.4412 1.44827L16.4601 1.44182L16.4614 1.44139C16.607 1.39065 16.8096 1.32136 17.0963 1.32136Z M8.92758 8.96557C8.43644 8.74349 8.08174 8.51976 7.71292 8.25621C7.14966 7.87099 6.63664 7.43272 6.20184 6.97839C5.14397 5.89888 4.66099 4.76909 4.81485 3.86797L4.42797 4.22703C3.65871 5.037 2.63779 6.45344 2.62887 7.06967L2.64769 7.13471C2.78425 7.60608 3.0165 8.07994 3.33745 8.54319L3.34276 8.55075C3.75676 9.22312 4.58365 10.3202 7.51246 11.6062L6.99454 12.5801L10.9472 11.5176L9.52333 7.85452L8.92758 8.96557Z M15.0736 15.1C15.565 15.3221 15.9195 15.5459 16.2883 15.8093C16.8517 16.1946 17.3646 16.6328 17.7994 17.0872C18.8574 18.1666 19.292 19.1879 19.1381 20.0886L19.5734 19.8386C20.3426 19.0286 21.3634 17.6121 21.3725 16.9959L21.3536 16.9309C21.217 16.4596 20.9849 15.9855 20.6638 15.5222L20.6584 15.5148C20.2446 14.8425 19.4176 13.7454 16.4888 12.4593L17.0067 11.4855L13.054 12.548L14.4781 16.2111L15.0736 15.1Z M15.1662 8.93522C15.388 8.44358 15.6118 8.08878 15.8751 7.71972C16.2599 7.15611 16.698 6.64274 17.1519 6.20766C18.2309 5.14896 19.3526 4.65805 20.2531 4.81202L19.9016 4.4325C19.0922 3.66274 17.6767 2.6413 17.061 2.63223L16.9959 2.65121C16.5247 2.78786 16.0513 3.02011 15.5883 3.34143L15.5807 3.34674C14.9089 3.761 13.8125 4.58842 12.5272 7.51916L11.554 7.0009L12.6158 10.9561L16.2766 9.5312L15.1662 8.93522Z M8.78405 15.0645C8.56209 15.5561 8.33846 15.9109 8.07513 16.2798C7.69014 16.8436 7.25215 17.3569 6.79825 17.792C5.71931 18.8507 4.6004 19.2986 3.69987 19.1447L4.04868 19.5671C4.85797 20.3368 6.2735 21.3584 6.88921 21.3673L6.95433 21.3484C7.42553 21.2118 7.89896 20.9795 8.36195 20.6581L8.36948 20.6529C9.04135 20.2386 10.1377 19.4113 11.4231 16.4805L12.3962 16.9987L11.3344 13.0435L7.67368 14.4683L8.78405 15.0645Z",
            new Rect(0, 0, 24, 24)
    ));

    // From Design System "Custom Icons" section on Figma
    public static readonly IconValue Stethoscope = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M13.8943 22.8744C12.0943 22.8744 10.561 22.2411 9.29429 20.9744C8.02762 19.7077 7.39429 18.1744 7.39429 16.3744V15.7994C5.96095 15.5661 4.76929 14.8952 3.81929 13.7869C2.86929 12.6786 2.39429 11.3744 2.39429 9.87439V3.87439H5.39429V2.87439H7.39429V6.87439H5.39429V5.87439H4.39429V9.87439C4.39429 10.9744 4.78595 11.9161 5.56929 12.6994C6.35262 13.4827 7.29429 13.8744 8.39429 13.8744C9.49429 13.8744 10.436 13.4827 11.2193 12.6994C12.0026 11.9161 12.3943 10.9744 12.3943 9.87439V5.87439H11.3943V6.87439H9.39429V2.87439H11.3943V3.87439H14.3943V9.87439C14.3943 11.3744 13.9193 12.6786 12.9693 13.7869C12.0193 14.8952 10.8276 15.5661 9.39429 15.7994V16.3744C9.39429 17.6244 9.83179 18.6869 10.7068 19.5619C11.5818 20.4369 12.6443 20.8744 13.8943 20.8744C15.1443 20.8744 16.2068 20.4369 17.0818 19.5619C17.9568 18.6869 18.3943 17.6244 18.3943 16.3744V14.6994C17.811 14.4994 17.3318 14.1411 16.9568 13.6244C16.5818 13.1077 16.3943 12.5244 16.3943 11.8744C16.3943 11.0411 16.686 10.3327 17.2693 9.74939C17.8526 9.16606 18.561 8.87439 19.3943 8.87439C20.2276 8.87439 20.936 9.16606 21.5193 9.74939C22.1026 10.3327 22.3943 11.0411 22.3943 11.8744C22.3943 12.5244 22.2068 13.1077 21.8318 13.6244C21.4568 14.1411 20.9776 14.4994 20.3943 14.6994V16.3744C20.3943 18.1744 19.761 19.7077 18.4943 20.9744C17.2276 22.2411 15.6943 22.8744 13.8943 22.8744ZM19.3943 12.8744C19.6776 12.8744 19.9151 12.7786 20.1068 12.5869C20.2985 12.3952 20.3943 12.1577 20.3943 11.8744C20.3943 11.5911 20.2985 11.3536 20.1068 11.1619C19.9151 10.9702 19.6776 10.8744 19.3943 10.8744C19.111 10.8744 18.8735 10.9702 18.6818 11.1619C18.4901 11.3536 18.3943 11.5911 18.3943 11.8744C18.3943 12.1577 18.4901 12.3952 18.6818 12.5869C18.8735 12.7786 19.111 12.8744 19.3943 12.8744Z",
        new Rect(0, 0, 25, 25)
    ));

    // From Design System "Custom Icons" section on Figma
    public static readonly IconValue ShieldHalfFull = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M21.4751 11.1256C21.4751 16.6756 17.6351 21.8656 12.4751 23.1256C7.3151 21.8656 3.4751 16.6756 3.4751 11.1256V5.12561L12.4751 1.12561L21.4751 5.12561V11.1256ZM12.4751 21.1256C16.2251 20.1256 19.4751 15.6656 19.4751 11.3456V6.42561L12.4751 3.30561V21.1256Z",
        new Rect(0, 0, 25, 25)
    ));
    
    // Game Stores from Design System "Custom Icons" section on Figma
    
    // Steam 
    public static readonly IconValue Steam = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M45.0034 3.82173C23.8216 4.40158 6.43405 20.3511 4.10415 41.3386C4.00893 42.1972 3.92912 43.1931 3.95373 43.2177C3.97282 43.2366 5.01058 43.6649 18.6742 49.2936L26.5956 52.5568L27.0117 52.3054C28.773 51.2412 30.886 50.6469 32.9284 50.6416L33.5225 50.64L38.6784 43.1383C41.5141 39.0121 43.8307 35.6073 43.8265 35.5719C43.7762 35.143 43.9673 33.1535 44.1405 32.3036C46.4308 21.0771 59.2264 15.7542 68.7125 22.0817C77.0355 27.6335 78.2946 39.4087 71.3331 46.5934C68.3638 49.658 64.5419 51.3391 60.2216 51.4811L59.4359 51.5069L52.0817 56.7714L44.7274 62.0356L44.7111 62.6021C44.4823 70.6165 36.6317 76.0416 29.1038 73.3873C25.533 72.1282 22.7229 69.077 21.7562 65.4093C21.6828 65.1317 21.618 64.9004 21.612 64.8953C21.6058 64.8902 20.9627 64.623 20.1825 64.3014C17.612 63.2414 14.456 61.9387 12.9759 61.3269C7.53437 59.0774 5.69299 58.3213 5.68264 58.3316C5.6647 58.3496 6.23349 60.0447 6.46924 60.6747C13.2717 78.864 31.4369 90.0686 50.6575 87.9308C74.8784 85.2369 91.7746 62.5803 87.4925 38.5381C83.858 18.1311 65.6531 3.25661 45.0034 3.82173ZM58.6116 24.8009C50.2725 25.8626 46.3264 35.4476 51.4956 42.0842C54.87 46.4162 61.212 47.4088 65.78 44.3196C71.9781 40.1283 72.1057 31.1073 66.0291 26.7129C63.9722 25.2254 61.0975 24.4844 58.6116 24.8009ZM61.2566 27.5011C67.6216 28.644 70.0882 36.5161 65.5153 41.0928C61.149 45.4627 53.6402 43.409 52.1146 37.4277C50.6736 31.7783 55.5556 26.4774 61.2566 27.5011ZM31.8654 53.3892C31.22 53.4769 29.9 53.8215 29.9 53.9023C29.9 53.9179 31.0601 54.4098 32.4779 54.9957C36.289 56.5704 36.708 56.8029 37.6329 57.8549C41.5661 62.3289 38.0572 69.4266 32.1676 68.9103C31.0169 68.8095 31.1565 68.8585 26.4691 66.907C25.6259 66.5559 24.9297 66.2737 24.9219 66.2799C24.8552 66.3342 25.706 67.6643 26.1347 68.1753C30.8354 73.7784 39.9462 71.6899 41.7639 64.5921C43.3421 58.429 38.1356 52.5361 31.8654 53.3892Z M45.6838 3.78828C45.8577 3.79542 46.1424 3.79542 46.3163 3.78828C46.4902 3.78115 46.3478 3.77539 46 3.77539C45.652 3.77539 45.5099 3.78115 45.6838 3.78828ZM44.2477 3.82649C44.3698 3.83432 44.5595 3.83409 44.6693 3.82649C44.7792 3.81867 44.6794 3.81245 44.4475 3.81245C44.2157 3.81268 44.1258 3.8189 44.2477 3.82649ZM47.3527 3.82649C47.4748 3.83432 47.6645 3.83409 47.7743 3.82649C47.8842 3.81867 47.7844 3.81245 47.5525 3.81245C47.3207 3.81268 47.2308 3.8189 47.3527 3.82649ZM43.4413 3.86448C43.5204 3.87276 43.6497 3.87276 43.7288 3.86448C43.8079 3.85619 43.7433 3.84951 43.585 3.84951C43.427 3.84951 43.3622 3.85619 43.4413 3.86448ZM48.2713 3.86448C48.3504 3.87276 48.4797 3.87276 48.5588 3.86448C48.6379 3.85619 48.5733 3.84951 48.415 3.84951C48.257 3.84951 48.1922 3.85619 48.2713 3.86448ZM42.8472 3.90269C42.9157 3.9112 43.0277 3.9112 43.0963 3.90269C43.1648 3.89417 43.1087 3.88726 42.9716 3.88726C42.8345 3.88726 42.7787 3.89417 42.8472 3.90269ZM48.9038 3.90269C48.9723 3.9112 49.0843 3.9112 49.1529 3.90269C49.2214 3.89417 49.1653 3.88726 49.0285 3.88726C48.8914 3.88726 48.8352 3.89417 48.9038 3.90269ZM42.3679 3.94067C42.4258 3.94965 42.5208 3.94965 42.5788 3.94067C42.6367 3.93192 42.5894 3.92479 42.4734 3.92479C42.3573 3.92479 42.3099 3.93192 42.3679 3.94067ZM49.4213 3.94067C49.4792 3.94965 49.5742 3.94965 49.6322 3.94067C49.6902 3.93192 49.6428 3.92479 49.5266 3.92479C49.4107 3.92479 49.3633 3.93192 49.4213 3.94067ZM41.9467 3.97911C42.0052 3.98809 42.0914 3.98786 42.1386 3.97865C42.1857 3.96944 42.1379 3.96231 42.0325 3.96254C41.9272 3.963 41.8886 3.97036 41.9467 3.97911ZM49.8817 3.97911C49.9402 3.98809 50.0264 3.98786 50.0736 3.97865C50.1207 3.96944 50.0729 3.96231 49.9675 3.96254C49.8622 3.963 49.8236 3.97036 49.8817 3.97911ZM41.5438 4.01709C41.5912 4.0263 41.6689 4.0263 41.7163 4.01709C41.7637 4.00788 41.7248 4.00052 41.63 4.00052C41.5353 4.00052 41.4964 4.00788 41.5438 4.01709ZM50.2838 4.01709C50.3312 4.0263 50.4089 4.0263 50.4563 4.01709C50.5037 4.00788 50.4648 4.00052 50.37 4.00052C50.2753 4.00052 50.2364 4.00788 50.2838 4.01709ZM41.1988 4.05553C41.2462 4.06451 41.3239 4.06451 41.3713 4.05553C41.4187 4.04633 41.3798 4.03873 41.285 4.03873C41.1903 4.03873 41.1514 4.04633 41.1988 4.05553ZM50.6288 4.05553C50.6762 4.06451 50.7539 4.06451 50.8013 4.05553C50.8487 4.04633 50.8098 4.03873 50.715 4.03873C50.6203 4.03873 50.5814 4.04633 50.6288 4.05553ZM40.8927 4.09398C40.9403 4.10295 41.0093 4.10272 41.0458 4.09306C41.0826 4.08339 41.0435 4.07602 40.9591 4.07648C40.8749 4.07694 40.8448 4.08477 40.8927 4.09398ZM50.9742 4.09398C51.0219 4.10295 51.0909 4.10272 51.1277 4.09306C51.1642 4.08339 51.1251 4.07602 51.041 4.07648C50.9565 4.07694 50.9266 4.08477 50.9742 4.09398ZM40.5663 4.1315C40.6031 4.14117 40.6636 4.14117 40.7004 4.1315C40.7374 4.12183 40.7071 4.114 40.6334 4.114C40.5596 4.114 40.5295 4.12183 40.5663 4.1315ZM51.2997 4.1315C51.3365 4.14117 51.397 4.14117 51.4338 4.1315C51.4706 4.12183 51.4405 4.114 51.3666 4.114C51.2928 4.114 51.2627 4.12183 51.2997 4.1315ZM40.2792 4.17063C40.3269 4.17984 40.3959 4.17938 40.4327 4.16994C40.4692 4.16027 40.4301 4.15268 40.3459 4.15314C40.2615 4.1536 40.2316 4.16142 40.2792 4.17063ZM51.5877 4.17063C51.6353 4.17984 51.7043 4.17938 51.7408 4.16994C51.7776 4.16027 51.7385 4.15268 51.6541 4.15314C51.5699 4.1536 51.5398 4.16142 51.5877 4.17063ZM39.9913 4.20838C40.0281 4.21782 40.0886 4.21782 40.1254 4.20838C40.1624 4.19871 40.132 4.19066 40.0584 4.19066C39.9846 4.19066 39.9545 4.19871 39.9913 4.20838ZM51.8747 4.20838C51.9115 4.21782 51.972 4.21782 52.0088 4.20838C52.0456 4.19871 52.0155 4.19066 51.9416 4.19066C51.8678 4.19066 51.8377 4.19871 51.8747 4.20838ZM39.7229 4.24659C39.7599 4.25626 39.8202 4.25626 39.8572 4.24659C39.894 4.23693 39.8639 4.2291 39.79 4.2291C39.7162 4.2291 39.6861 4.23693 39.7229 4.24659ZM52.1429 4.24659C52.1799 4.25626 52.2402 4.25626 52.2772 4.24659C52.314 4.23693 52.2839 4.2291 52.21 4.2291C52.1362 4.2291 52.1061 4.23693 52.1429 4.24659ZM38.8509 4.36698C38.6612 4.40036 38.5317 4.43259 38.5634 4.4388C38.595 4.44502 38.802 4.41763 39.0234 4.37757C39.2447 4.33775 39.3742 4.30529 39.3109 4.30575C39.2477 4.30598 39.0407 4.33338 38.8509 4.36698ZM52.8425 4.35639C52.9902 4.38448 53.154 4.40635 53.2066 4.40543C53.2595 4.40428 53.1818 4.38057 53.0341 4.35248C52.8867 4.32463 52.7227 4.30253 52.67 4.30368C52.6174 4.3046 52.6949 4.32854 52.8425 4.35639ZM38.1991 4.48876C38.0965 4.51477 38.0945 4.51799 38.18 4.51799C38.2327 4.51799 38.3277 4.50487 38.3909 4.48876C38.4935 4.46274 38.4956 4.45952 38.41 4.45952C38.3574 4.45952 38.2624 4.47264 38.1991 4.48876ZM53.6091 4.48876C53.6724 4.50487 53.7674 4.51799 53.82 4.51799C53.9056 4.51799 53.9035 4.51477 53.8009 4.48876C53.7377 4.47264 53.6427 4.45952 53.59 4.45952C53.5045 4.45952 53.5065 4.46274 53.6091 4.48876ZM54.3375 4.62526C54.464 4.6538 54.5933 4.6759 54.625 4.67429C54.715 4.66992 54.3109 4.57278 54.2034 4.57301C54.1508 4.57324 54.211 4.59672 54.3375 4.62526ZM37.4134 4.64229C37.2844 4.6835 37.4238 4.6835 37.5859 4.64229C37.6892 4.61582 37.6911 4.61283 37.605 4.61214C37.5524 4.61191 37.4661 4.62526 37.4134 4.64229ZM36.9725 4.73736C36.8672 4.76429 36.8154 4.78685 36.8575 4.78754C36.8996 4.788 37.0204 4.76614 37.1259 4.73897C37.2313 4.71181 37.283 4.68925 37.2409 4.68879C37.1986 4.68833 37.0779 4.7102 36.9725 4.73736ZM54.97 4.75739C55.0227 4.77419 55.1004 4.788 55.1425 4.788C55.2033 4.788 55.1993 4.78156 55.1235 4.75739C55.0708 4.74036 54.993 4.72654 54.951 4.72654C54.8902 4.72654 54.8941 4.73299 54.97 4.75739ZM36.455 4.85315C36.36 4.88077 36.3083 4.90471 36.34 4.90632C36.3715 4.90793 36.4838 4.88538 36.5891 4.85591C36.6947 4.82645 36.7464 4.80274 36.7041 4.80297C36.662 4.80297 36.5498 4.82576 36.455 4.85315ZM56.0591 5.00991C56.5212 5.12455 56.9059 5.2111 56.9145 5.20281C56.9331 5.18393 55.3507 4.79445 55.2736 4.79882C55.2437 4.80044 55.5972 4.8955 56.0591 5.00991ZM35.995 4.9542C35.9212 4.97423 35.5332 5.07736 35.1325 5.18301C20.8183 8.96299 9.43394 20.1812 5.3887 34.4929C5.15893 35.3061 4.82957 36.6226 4.83279 36.7156C4.83371 36.7472 4.94342 36.3415 5.07636 35.8139C8.89298 20.6724 20.421 9.05806 35.5562 5.10613C35.9557 5.00185 36.248 4.91668 36.2059 4.91714C36.1636 4.91737 36.0689 4.93418 35.995 4.9542ZM57.021 5.25092C57.063 5.26934 57.1578 5.29535 57.2316 5.30893C57.3239 5.3255 57.3418 5.32274 57.2891 5.29949C57.247 5.28108 57.1521 5.25506 57.0784 5.24171C56.9862 5.22491 56.9683 5.2279 57.021 5.25092ZM57.596 5.40998C57.7225 5.45142 57.8432 5.48434 57.8641 5.48295C57.9258 5.4795 57.644 5.38305 57.4987 5.35773C57.4255 5.34507 57.4695 5.36855 57.596 5.40998ZM58.0366 5.54372C58.1105 5.57227 58.1967 5.59552 58.2285 5.59552C58.26 5.59552 58.2255 5.57227 58.1516 5.54372C58.0778 5.51541 57.9916 5.49216 57.96 5.49216C57.9283 5.49216 57.9628 5.51541 58.0366 5.54372ZM58.9566 5.82847C72.8758 10.4005 83.5574 21.8503 86.9244 35.8073C86.9847 36.0568 87.0394 36.2444 87.0461 36.2244C87.053 36.2041 87.0047 35.9739 86.9389 35.7131C83.646 22.6385 74.3717 11.9559 61.8891 6.86019C60.7851 6.40948 58.4601 5.59137 58.3209 5.60473C58.3016 5.60657 58.5877 5.70716 58.9566 5.82847ZM59.5797 19.4029C59.7218 19.4103 59.9548 19.4103 60.0972 19.4029C60.2393 19.3955 60.123 19.3895 59.8384 19.3895C59.5537 19.3895 59.4373 19.3955 59.5797 19.4029ZM58.7565 19.4411C58.8467 19.4492 58.9847 19.4489 59.0631 19.4406C59.1415 19.4326 59.068 19.4259 58.8991 19.4259C58.7305 19.4261 58.6664 19.4328 58.7565 19.4411ZM60.6347 19.4411C60.7249 19.4492 60.8629 19.4489 60.9415 19.4406C61.02 19.4326 60.9461 19.4259 60.7775 19.4259C60.6089 19.4261 60.5445 19.4328 60.6347 19.4411ZM58.2763 19.4786C58.3342 19.4876 58.4292 19.4876 58.4872 19.4786C58.5452 19.4699 58.4978 19.4627 58.3816 19.4627C58.2657 19.4627 58.2183 19.4699 58.2763 19.4786ZM61.1897 19.4786C61.2477 19.4876 61.3424 19.4876 61.4004 19.4786C61.4583 19.4699 61.411 19.4627 61.295 19.4627C61.1791 19.4627 61.1317 19.4699 61.1897 19.4786ZM57.6341 19.5334C52.6275 20.2871 48.4219 23.1566 45.9214 27.525C44.8956 29.317 44.0694 31.8611 43.9521 33.5889C43.9367 33.815 43.95 33.7593 44.0029 33.3778C44.9963 26.206 50.4526 20.6618 57.5766 19.5859C57.9035 19.5364 58.1019 19.4968 58.0175 19.4975C57.9331 19.4982 57.7606 19.5145 57.6341 19.5334ZM61.5922 19.5166C61.6396 19.5258 61.7171 19.5258 61.7647 19.5166C61.8121 19.5076 61.7732 19.5 61.6784 19.5C61.5835 19.5 61.5446 19.5076 61.5922 19.5166ZM61.9179 19.5543C61.9549 19.564 62.0152 19.564 62.0522 19.5543C62.089 19.5447 62.0589 19.5369 61.985 19.5369C61.9112 19.5369 61.8811 19.5447 61.9179 19.5543ZM62.6352 19.6667C69.2595 20.8618 74.4437 26.1121 75.5914 32.7874C75.6526 33.1437 75.7041 33.3919 75.7055 33.3385C75.7098 33.1888 75.5279 32.1836 75.4035 31.6705C74.0074 25.9095 69.5507 21.3729 63.8303 19.8897C63.2898 19.7496 62.3443 19.5638 62.2028 19.57C62.1568 19.5718 62.3514 19.6156 62.6352 19.6667ZM59.1004 24.7353C59.1901 24.7433 59.3366 24.7433 59.4263 24.7353C59.516 24.7272 59.4426 24.7208 59.2634 24.7208C59.084 24.7208 59.0109 24.7272 59.1004 24.7353ZM60.2708 24.7355C60.3716 24.7433 60.5268 24.7433 60.6158 24.7353C60.7049 24.7272 60.6223 24.7208 60.4325 24.7208C60.2428 24.721 60.1701 24.7277 60.2708 24.7355ZM58.6602 24.7733C58.7183 24.782 58.8046 24.7818 58.8517 24.7726C58.8989 24.7636 58.8513 24.7562 58.7459 24.7567C58.6404 24.7569 58.602 24.7643 58.6602 24.7733ZM60.7998 24.7601C60.8086 24.7682 60.9539 24.7841 61.1225 24.7958C61.353 24.8117 61.3864 24.8087 61.2566 24.7841C61.0795 24.7505 60.7713 24.7344 60.7998 24.7601ZM57.826 24.878C53.5176 25.6986 50.1117 29.1269 49.3148 33.4444C49.2628 33.727 49.2214 34.0046 49.223 34.0613C49.2246 34.1179 49.2676 33.9227 49.3185 33.6271C50.0897 29.1557 53.5684 25.6719 58.056 24.878C58.3299 24.8297 58.4939 24.7901 58.42 24.7901C58.3462 24.7903 58.0789 24.8297 57.826 24.878ZM61.9473 24.9339C66.2748 25.8262 69.6105 29.2661 70.3764 33.6271C70.4171 33.8594 70.4518 33.9975 70.4532 33.9342C70.4548 33.8709 70.4042 33.5631 70.3409 33.2505C69.5677 29.4321 66.7017 26.2835 62.9688 25.1512C62.5111 25.0124 61.6577 24.8207 61.5149 24.8248C61.4678 24.826 61.6623 24.8752 61.9473 24.9339ZM59.5431 27.4214C59.7287 27.4285 60.022 27.4285 60.195 27.4214C60.3677 27.4143 60.2159 27.4085 59.8575 27.4085C59.4992 27.4085 59.3577 27.4145 59.5431 27.4214ZM58.9472 27.4582C58.9946 27.4674 59.0721 27.4674 59.1197 27.4582C59.1671 27.4492 59.1282 27.4416 59.0334 27.4416C58.9385 27.4416 58.8996 27.4492 58.9472 27.4582ZM60.9116 27.493C64.4184 27.9623 67.1754 30.6966 67.7548 34.2795C67.8004 34.5606 67.8075 34.5804 67.7928 34.3861C67.6573 32.5857 66.4355 30.3936 64.8791 29.1582C63.6615 28.1916 61.727 27.4046 60.6434 27.4352C60.5696 27.4375 60.6904 27.4633 60.9116 27.493ZM58.6597 27.496C58.6965 27.5056 58.757 27.5056 58.7938 27.496C58.8306 27.4863 58.8005 27.4785 58.7266 27.4785C58.6528 27.4785 58.6227 27.4863 58.6597 27.496ZM58.075 27.5897C54.8127 28.3396 52.1323 31.3464 51.9241 34.4894C51.9074 34.7454 51.9115 34.7325 51.9619 34.3752C52.4334 31.029 54.9079 28.3343 58.1778 27.6067C58.406 27.556 58.5495 27.5146 58.4966 27.5146C58.444 27.5148 58.2542 27.5487 58.075 27.5897ZM75.7358 33.7998C75.7471 34.0003 75.763 34.1715 75.7708 34.1803C75.7961 34.2086 75.7793 33.8253 75.7466 33.6271C75.7241 33.4904 75.7209 33.5399 75.7358 33.7998ZM43.9036 34.126C43.9038 34.2526 43.9109 34.2997 43.9195 34.2307C43.928 34.1619 43.9277 34.0583 43.919 34.0005C43.9103 33.943 43.9034 33.9993 43.9036 34.126ZM70.4672 34.2219C70.4672 34.3168 70.4746 34.3557 70.4838 34.3083C70.493 34.2606 70.493 34.183 70.4838 34.1356C70.4746 34.088 70.4672 34.1269 70.4672 34.2219ZM49.1927 34.3561C49.1929 34.4616 49.2005 34.5002 49.2092 34.442C49.218 34.3835 49.2177 34.2972 49.2088 34.25C49.1996 34.2028 49.1924 34.2507 49.1927 34.3561ZM75.7981 34.5097C75.7984 34.6784 75.8051 34.7426 75.8131 34.6524C75.8212 34.5622 75.8212 34.4241 75.8129 34.3456C75.8046 34.2671 75.7981 34.3407 75.7981 34.5097ZM43.8679 34.8356C43.8679 35.0783 43.8741 35.1777 43.8817 35.0564C43.8893 34.9351 43.8893 34.7364 43.8817 34.6151C43.8741 34.4938 43.8679 34.593 43.8679 34.8356ZM70.5072 34.6439C70.5072 34.7811 70.5144 34.8372 70.5229 34.7687C70.5314 34.7001 70.5314 34.5877 70.5229 34.5191C70.5144 34.4505 70.5072 34.5067 70.5072 34.6439ZM49.1598 35.4305C49.1598 35.8945 49.1653 36.0791 49.172 35.8407C49.1786 35.6022 49.1786 35.2224 49.172 34.9965C49.1653 34.771 49.1598 34.9662 49.1598 35.4305ZM67.8245 34.9125C67.8245 35.0707 67.8314 35.1353 67.8397 35.0564C67.848 34.9772 67.848 34.8476 67.8397 34.7687C67.8314 34.6895 67.8245 34.7542 67.8245 34.9125ZM51.8807 35.4111C51.8807 35.7594 51.8864 35.9019 51.8936 35.7276C51.9007 35.5536 51.9007 35.2689 51.8936 35.0946C51.8864 34.9206 51.8807 35.0631 51.8807 35.4111ZM75.8382 35.4305C75.8384 35.7046 75.8444 35.8119 75.8517 35.6685C75.8593 35.5253 75.8591 35.3009 75.8517 35.1699C75.8442 35.0389 75.8382 35.1561 75.8382 35.4305ZM70.5477 35.4305C70.5477 35.6625 70.5542 35.7525 70.5617 35.6305C70.5693 35.5083 70.5693 35.3183 70.5615 35.2083C70.5539 35.0983 70.5477 35.1982 70.5477 35.4305ZM43.8479 35.5854C43.8362 35.6284 41.63 38.8569 38.9452 42.7598C36.2607 46.6624 33.9457 50.0283 33.8008 50.2394L33.5373 50.6231L32.8974 50.6436L32.2575 50.6643L32.9091 50.6712L33.5609 50.6781L37.5475 44.8784C39.7401 41.6887 42.0659 38.3055 42.7159 37.3604C43.4302 36.3213 43.8919 35.6151 43.8833 35.5746C43.8714 35.5161 43.8666 35.5175 43.8479 35.5854ZM67.8245 35.9099C67.8245 36.0683 67.8314 36.133 67.8397 36.0538C67.848 35.9746 67.848 35.8453 67.8397 35.7661C67.8314 35.6869 67.8245 35.7518 67.8245 35.9099ZM70.5072 36.2168C70.5072 36.354 70.5144 36.4101 70.5229 36.3416C70.5314 36.273 70.5314 36.1609 70.5229 36.0923C70.5144 36.0237 70.5072 36.0796 70.5072 36.2168ZM51.9244 36.3328C52.0851 38.7733 53.8725 41.3604 56.2258 42.5588C57.0904 42.9992 58.4716 43.4041 59.0525 43.3877C59.1158 43.3859 58.9605 43.3525 58.7075 43.3136C55.1888 42.772 52.464 40.0048 51.9649 36.4663C51.9143 36.1084 51.9083 36.0881 51.9244 36.3328ZM75.7981 36.3512C75.7984 36.52 75.8051 36.5842 75.8131 36.4939C75.8212 36.4037 75.8212 36.2656 75.8129 36.1871C75.8046 36.1086 75.7981 36.1823 75.7981 36.3512ZM67.7426 36.569C67.3084 39.8046 64.6871 42.5873 61.4659 43.2328C61.2559 43.2749 61.0411 43.318 60.9884 43.3286C60.9358 43.3392 60.996 43.3389 61.1225 43.3276C63.1705 43.1483 65.5227 41.5469 66.75 39.497C67.2971 38.5832 67.8682 36.8526 67.8045 36.301C67.7978 36.2442 67.77 36.3648 67.7426 36.569ZM87.0592 36.3319C87.0578 36.3636 87.0806 36.4758 87.1098 36.5814C87.1392 36.6868 87.1643 36.7472 87.1659 36.7156C87.1673 36.6838 87.1445 36.5717 87.1153 36.4663C87.0859 36.3607 87.0608 36.3003 87.0592 36.3319ZM49.1927 36.5045C49.1929 36.6102 49.2005 36.6486 49.2092 36.5904C49.218 36.5322 49.2177 36.4458 49.2088 36.3986C49.1996 36.3514 49.1924 36.3991 49.1927 36.5045ZM70.4672 36.639C70.4672 36.7338 70.4746 36.7727 70.4838 36.7253C70.493 36.6776 70.493 36.6001 70.4838 36.5526C70.4746 36.505 70.4672 36.5439 70.4672 36.639ZM75.7321 37.0176C75.7172 37.2154 75.7126 37.385 75.7222 37.3945C75.7462 37.4184 75.7975 36.772 75.7765 36.7092C75.7671 36.6811 75.7471 36.8199 75.7321 37.0176ZM49.2253 36.7994C49.2223 36.856 49.2628 37.1336 49.3148 37.4163C50.0924 41.63 53.3437 44.9979 57.527 45.9237C57.7123 45.9647 57.8986 45.9972 57.9409 45.9958C57.983 45.9944 57.819 45.9505 57.5766 45.8977C53.1788 44.945 49.9993 41.4695 49.246 36.7923C49.2306 36.697 49.2304 36.697 49.2253 36.7994ZM4.7746 36.9288C4.76034 37.0057 4.7562 37.0763 4.7654 37.0858C4.77483 37.0952 4.79438 37.04 4.80864 36.9628C4.82313 36.886 4.82727 36.8153 4.81784 36.8058C4.80864 36.7964 4.78909 36.8517 4.7746 36.9288ZM87.1783 36.8498C87.1779 36.8922 87.1995 37.013 87.2266 37.1185C87.2538 37.2239 87.2763 37.2757 87.2768 37.2335C87.2772 37.1914 87.2556 37.0706 87.2285 36.9649C87.2013 36.8595 87.1788 36.8077 87.1783 36.8498ZM70.3874 37.1548C69.745 41.5559 66.071 45.241 61.5683 46.0009C61.3443 46.0386 61.204 46.0711 61.2566 46.0731C61.4036 46.0784 61.9933 45.9691 62.4834 45.8459C66.4463 44.8497 69.5346 41.6079 70.3435 37.5954C70.4081 37.2752 70.4557 36.9677 70.4493 36.9122C70.4428 36.857 70.415 36.9661 70.3874 37.1548ZM4.61912 37.6748C4.58209 37.8661 4.55932 38.0305 4.56852 38.0397C4.57795 38.0491 4.61567 37.8997 4.65224 37.708C4.68904 37.516 4.71181 37.3519 4.70284 37.3429C4.6941 37.3339 4.65638 37.4833 4.61912 37.6748ZM87.2913 37.3677C87.2896 37.3995 87.3117 37.5289 87.3402 37.6555C87.3688 37.7821 87.3922 37.8426 87.3925 37.7899C87.3927 37.6822 87.2959 37.2777 87.2913 37.3677ZM75.5843 38.0862C74.5113 44.6722 69.3004 49.9558 62.6559 51.1951C62.3818 51.2462 62.2008 51.2895 62.2534 51.2911C62.4172 51.296 63.6254 51.037 64.1995 50.874C69.7448 49.3002 74.0566 44.8018 75.4042 39.184C75.5392 38.6211 75.7174 37.6203 75.7011 37.5169C75.6944 37.4759 75.642 37.7321 75.5843 38.0862ZM4.50596 38.2712C4.49124 38.3691 4.4871 38.457 4.49676 38.4665C4.50619 38.4761 4.5262 38.4038 4.54069 38.306C4.55541 38.2082 4.55955 38.1202 4.55012 38.1106C4.54046 38.1011 4.52068 38.1732 4.50596 38.2712ZM87.4474 38.1734C87.4474 38.2264 87.4605 38.3212 87.4766 38.3845C87.5029 38.4872 87.5061 38.4892 87.5061 38.4036C87.5061 38.3509 87.4927 38.2561 87.4766 38.1928C87.4506 38.0901 87.4474 38.088 87.4474 38.1734ZM4.41051 38.8087C4.38452 38.9692 4.37118 39.1082 4.38038 39.1174C4.38981 39.1269 4.41879 39.0033 4.44455 38.8428C4.47054 38.6821 4.48388 38.5433 4.47468 38.5339C4.46525 38.5245 4.43627 38.6481 4.41051 38.8087ZM87.5219 38.5763C87.5208 38.6186 87.5525 38.8258 87.5926 39.0366C87.6344 39.2567 87.657 39.3304 87.6452 39.2093C87.6211 38.9591 87.5252 38.466 87.5219 38.5763ZM4.34059 39.2862C4.34059 39.3598 4.34841 39.3902 4.35807 39.3531C4.36773 39.3163 4.36773 39.2558 4.35807 39.2189C4.34841 39.1821 4.34059 39.2123 4.34059 39.2862ZM4.264 39.7848C4.264 39.8587 4.27182 39.8888 4.28148 39.852C4.29114 39.8151 4.29114 39.7546 4.28148 39.7178C4.27182 39.6807 4.264 39.7109 4.264 39.7848ZM87.7156 39.7848C87.7156 39.8587 87.7234 39.8888 87.7331 39.852C87.7427 39.8151 87.7427 39.7546 87.7331 39.7178C87.7234 39.6807 87.7156 39.7109 87.7156 39.7848ZM4.22559 40.0534C4.22559 40.1273 4.23341 40.1574 4.24307 40.1206C4.25273 40.0835 4.25273 40.0232 4.24307 39.9862C4.23341 39.9493 4.22559 39.9795 4.22559 40.0534ZM87.754 40.0534C87.754 40.1273 87.7618 40.1574 87.7715 40.1206C87.7812 40.0835 87.7812 40.0232 87.7715 39.9862C87.7618 39.9493 87.754 39.9795 87.754 40.0534ZM4.1881 40.3411C4.18856 40.4256 4.19638 40.4555 4.20558 40.4079C4.21455 40.36 4.21432 40.291 4.20466 40.2544C4.195 40.2178 4.18764 40.2567 4.1881 40.3411ZM87.7931 40.3411C87.7936 40.4256 87.8014 40.4555 87.8106 40.4079C87.8196 40.36 87.8193 40.291 87.8097 40.2544C87.8 40.2178 87.7927 40.2567 87.7931 40.3411ZM4.149 40.6289C4.149 40.7028 4.15682 40.7329 4.16648 40.6961C4.17614 40.659 4.17614 40.5987 4.16648 40.5617C4.15682 40.5248 4.149 40.555 4.149 40.6289ZM87.8313 40.648C87.8317 40.7325 87.8396 40.7624 87.8488 40.7147C87.858 40.6671 87.8577 40.598 87.8481 40.5612C87.8384 40.5246 87.8311 40.5637 87.8313 40.648ZM4.11128 40.9551C4.11174 41.0393 4.11956 41.0695 4.12876 41.0216C4.13796 40.9739 4.13773 40.9049 4.12807 40.8683C4.11841 40.8314 4.11105 40.8706 4.11128 40.9551ZM87.8697 40.9551C87.8702 41.0393 87.878 41.0695 87.8872 41.0216C87.8964 40.9739 87.8959 40.9049 87.8865 40.8683C87.8768 40.8314 87.8692 40.8706 87.8697 40.9551ZM4.07379 41.281C4.07379 41.3761 4.08138 41.415 4.09035 41.3673C4.09955 41.3199 4.09955 41.2423 4.09035 41.1947C4.08138 41.1473 4.07379 41.1862 4.07379 41.281ZM87.9088 41.281C87.9088 41.3761 87.9164 41.415 87.9254 41.3673C87.9346 41.3199 87.9346 41.2423 87.9254 41.1947C87.9164 41.1473 87.9088 41.1862 87.9088 41.281ZM4.03538 41.6263C4.03538 41.7214 4.04297 41.7603 4.05217 41.7126C4.06114 41.6652 4.06114 41.5876 4.05217 41.54C4.04297 41.4926 4.03538 41.5315 4.03538 41.6263ZM87.9472 41.6263C87.9472 41.7214 87.9546 41.7603 87.9638 41.7126C87.973 41.6652 87.973 41.5876 87.9638 41.54C87.9546 41.4926 87.9472 41.5315 87.9472 41.6263ZM3.99766 42.0291C3.99789 42.1348 4.00548 42.1732 4.01422 42.115C4.02296 42.0568 4.02273 41.9704 4.01376 41.9232C4.00456 41.8761 3.99743 41.9237 3.99766 42.0291ZM87.9861 42.0291C87.9863 42.1348 87.9937 42.1732 88.0026 42.115C88.0114 42.0568 88.0112 41.9704 88.002 41.9232C87.993 41.8761 87.9856 41.9237 87.9861 42.0291ZM3.95994 42.4704C3.95994 42.5864 3.96707 42.6338 3.97581 42.5758C3.98455 42.5178 3.98455 42.423 3.97581 42.365C3.96707 42.307 3.95994 42.3544 3.95994 42.4704ZM88.025 42.4704C88.025 42.5864 88.0321 42.6338 88.0408 42.5758C88.0496 42.5178 88.0496 42.423 88.0408 42.365C88.0321 42.307 88.025 42.3544 88.025 42.4704ZM3.91762 42.9923L3.91003 43.2648L7.40787 44.7063C22.3091 50.8462 26.5703 52.5989 26.5972 52.5989C26.614 52.5989 26.8164 52.4849 27.0469 52.3459C28.2919 51.5941 30.5203 50.7992 31.3918 50.796C31.4429 50.7958 31.4735 50.7847 31.4599 50.7711C31.396 50.7071 30.1489 50.9774 29.4591 51.2046C28.5543 51.5024 27.6589 51.9117 26.9466 52.3525L26.6007 52.5666L23.9474 51.4732C22.4883 50.8717 17.4734 48.8055 12.8034 46.8818C8.13329 44.9581 4.23134 43.3493 4.13221 43.3069L3.95166 43.2296L3.93832 42.9748L3.92498 42.7197L3.91762 42.9923ZM88.0636 42.9883C88.0638 43.1149 88.071 43.1621 88.0795 43.0931C88.088 43.0243 88.0877 42.9207 88.079 42.8629C88.0703 42.8053 88.0634 42.8617 88.0636 42.9883ZM88.1029 43.5829C88.1029 43.7413 88.1096 43.806 88.1179 43.7268C88.1262 43.6478 88.1262 43.5182 88.1179 43.4391C88.1096 43.3601 88.1029 43.4248 88.1029 43.5829ZM60.6338 43.3799C60.6812 43.3891 60.7589 43.3891 60.8063 43.3799C60.8537 43.3707 60.8148 43.3633 60.72 43.3633C60.625 43.3633 60.5864 43.3707 60.6338 43.3799ZM59.5413 43.4197C59.7257 43.4269 60.0277 43.4269 60.2122 43.4197C60.3967 43.4128 60.2458 43.4071 59.8766 43.4071C59.5077 43.4071 59.3568 43.4128 59.5413 43.4197ZM88.1427 44.4461C88.1427 44.6784 88.1492 44.7684 88.1567 44.6462C88.1643 44.5242 88.1643 44.3343 88.1565 44.2242C88.1489 44.1142 88.1427 44.2141 88.1427 44.4461ZM88.1825 45.9999C88.1825 46.3694 88.1882 46.5204 88.1952 46.3358C88.2023 46.1512 88.2023 45.8489 88.1952 45.6643C88.1882 45.4797 88.1825 45.6307 88.1825 45.9999ZM58.3147 46.0649C58.3515 46.0745 58.412 46.0745 58.4488 46.0649C58.4856 46.0552 58.4555 46.0474 58.3816 46.0474C58.3078 46.0474 58.2777 46.0552 58.3147 46.0649ZM58.6217 46.1042C58.6802 46.1132 58.7664 46.113 58.8136 46.1038C58.8607 46.0945 58.8129 46.0874 58.7075 46.0876C58.6022 46.0881 58.5636 46.0955 58.6217 46.1042ZM60.8836 46.1042C60.9418 46.1132 61.028 46.113 61.0752 46.1038C61.1223 46.0945 61.0747 46.0874 60.9691 46.0876C60.8638 46.0881 60.8251 46.0955 60.8836 46.1042ZM59.0631 46.1433C59.1533 46.1514 59.2913 46.1512 59.3697 46.1429C59.4484 46.1348 59.3745 46.1282 59.2059 46.1282C59.0371 46.1284 58.973 46.1351 59.0631 46.1433ZM60.3281 46.1433C60.4183 46.1514 60.5563 46.1512 60.6347 46.1429C60.7134 46.1348 60.6395 46.1282 60.4709 46.1282C60.3021 46.1284 60.238 46.1351 60.3281 46.1433ZM88.1427 47.5537C88.1427 47.786 88.1492 47.876 88.1567 47.7538C88.1643 47.6318 88.1643 47.4419 88.1565 47.3318C88.1489 47.2218 88.1427 47.3217 88.1427 47.5537ZM88.1029 48.417C88.1029 48.5753 88.1096 48.64 88.1179 48.5608C88.1262 48.4819 88.1262 48.3523 88.1179 48.2731C88.1096 48.1941 88.1029 48.2588 88.1029 48.417ZM88.0636 49.0118C88.0638 49.1384 88.071 49.1856 88.0795 49.1165C88.088 49.0474 88.0877 48.9439 88.079 48.8863C88.0703 48.8288 88.0634 48.8852 88.0636 49.0118ZM88.025 49.5297C88.025 49.6457 88.0321 49.6931 88.0408 49.6351C88.0496 49.5771 88.0496 49.4821 88.0408 49.424C88.0321 49.366 88.025 49.4135 88.025 49.5297ZM87.9861 49.9707C87.9863 50.0764 87.9937 50.1148 88.0026 50.0566C88.0114 49.9984 88.0112 49.9121 88.002 49.8649C87.993 49.8177 87.9856 49.8653 87.9861 49.9707ZM87.9472 50.3736C87.9472 50.4687 87.9546 50.5076 87.9638 50.4599C87.973 50.4125 87.973 50.3349 87.9638 50.2873C87.9546 50.2398 87.9472 50.2787 87.9472 50.3736ZM87.9088 50.7189C87.9088 50.8139 87.9164 50.8528 87.9254 50.8052C87.9346 50.7578 87.9346 50.6802 87.9254 50.6325C87.9164 50.5851 87.9088 50.624 87.9088 50.7189ZM31.6634 50.7207C31.5602 50.738 31.6372 50.7414 31.855 50.7292C32.0554 50.7182 32.2265 50.7028 32.2352 50.6949C32.2614 50.6712 31.8488 50.6899 31.6634 50.7207ZM87.8697 51.0451C87.8702 51.1295 87.878 51.1595 87.8872 51.1118C87.8964 51.0639 87.8959 50.9949 87.8865 50.9583C87.8768 50.9217 87.8692 50.9606 87.8697 51.0451ZM87.8313 51.3519C87.8317 51.4364 87.8396 51.4663 87.8488 51.4187C87.858 51.371 87.8577 51.3019 87.8481 51.2651C87.8384 51.2285 87.8311 51.2677 87.8313 51.3519ZM61.9563 51.3208C61.9931 51.3305 62.0536 51.3305 62.0904 51.3208C62.1274 51.3112 62.097 51.3033 62.0234 51.3033C61.9496 51.3033 61.9195 51.3112 61.9563 51.3208ZM61.3716 51.3712C61.1619 51.3968 61.1616 51.397 61.3334 51.4C61.4282 51.4014 61.5922 51.3894 61.6975 51.3728C61.9535 51.333 61.6943 51.3316 61.3716 51.3712ZM60.7479 51.4373C60.8159 51.4456 60.9367 51.4458 61.0163 51.4375C61.0959 51.4292 61.0402 51.4223 60.8925 51.4221C60.7449 51.4219 60.6798 51.4288 60.7479 51.4373ZM52.1141 56.6852C44.0059 62.4849 44.6573 62.0114 44.661 62.1035C44.6637 62.1677 44.6651 62.1677 44.6904 62.1037C44.7051 62.0666 48.0344 59.663 52.0891 56.7623L59.4613 51.4884L59.9852 51.4767L60.5091 51.4647L59.9725 51.4564L59.4359 51.4481L52.1141 56.6852ZM87.7931 51.659C87.7936 51.7432 87.8014 51.7734 87.8106 51.7255C87.8196 51.6779 87.8193 51.6088 87.8097 51.5722C87.8 51.5354 87.7927 51.5745 87.7931 51.659ZM87.754 51.9467C87.754 52.0204 87.7618 52.0508 87.7715 52.0137C87.7812 51.9769 87.7812 51.9163 87.7715 51.8795C87.7618 51.8427 87.754 51.8728 87.754 51.9467ZM87.7156 52.2151C87.7156 52.289 87.7234 52.3192 87.7331 52.2823C87.7427 52.2455 87.7427 52.185 87.7331 52.1481C87.7234 52.1111 87.7156 52.1412 87.7156 52.2151ZM87.5769 53.0254C87.5397 53.2588 87.5169 53.4575 87.5261 53.4667C87.5353 53.4759 87.5735 53.2924 87.6109 53.0592C87.6484 52.826 87.6712 52.6274 87.6618 52.618C87.6524 52.6083 87.6142 52.7917 87.5769 53.0254ZM32.3447 53.3175C32.4349 53.3256 32.5729 53.3256 32.6515 53.3173C32.73 53.309 32.6561 53.3025 32.4875 53.3025C32.3189 53.3028 32.2545 53.3095 32.3447 53.3175ZM33.4172 53.3175C33.5067 53.3256 33.6534 53.3256 33.7429 53.3175C33.8326 53.3095 33.7592 53.3028 33.58 53.3028C33.4009 53.3028 33.3275 53.3095 33.4172 53.3175ZM31.5866 53.385C31.0096 53.4837 29.8294 53.8071 29.8761 53.8539C29.8855 53.8633 30.0801 53.8106 30.3087 53.7369C30.7598 53.5917 31.3412 53.4588 31.855 53.3838C32.0409 53.3566 32.1149 53.3362 32.0275 53.3359C31.9431 53.3357 31.7449 53.3578 31.5866 53.385ZM34.3659 53.4013C38.2695 54.0212 41.2144 56.9253 41.9364 60.8666C41.9847 61.1314 42.0059 61.203 41.993 61.0591C41.6537 57.289 37.9516 53.6384 34.1741 53.3488C33.9816 53.3341 34.0359 53.349 34.3659 53.4013ZM87.4594 53.6941C87.4447 53.7919 87.4405 53.8799 87.45 53.8893C87.4596 53.899 87.4794 53.8267 87.4941 53.7289C87.5088 53.631 87.513 53.5431 87.5033 53.5334C87.4939 53.524 87.4739 53.596 87.4594 53.6941ZM29.9 53.977C29.9315 53.995 31.1133 54.4876 32.5259 55.0713C33.9384 55.6549 35.3184 56.2423 35.5925 56.3765C37.5061 57.3139 38.8342 59.1165 39.1971 61.2695C39.2238 61.4276 39.2472 61.508 39.2495 61.4481C39.2516 61.388 39.2189 61.1694 39.1766 60.9622C38.7815 59.0219 37.5498 57.3647 35.8034 56.424C35.3568 56.1834 29.5511 53.7767 29.9 53.977ZM87.308 54.4825C87.294 54.5704 87.2899 54.6499 87.2991 54.6591C87.3083 54.6683 87.3278 54.6043 87.3425 54.517C87.3573 54.4298 87.3612 54.3504 87.3515 54.3407C87.3416 54.3308 87.3221 54.3946 87.308 54.4825ZM87.211 54.9417C87.1855 55.0709 87.1721 55.1844 87.1813 55.1933C87.1905 55.2025 87.2188 55.1043 87.2441 54.9749C87.2696 54.8458 87.283 54.7323 87.2738 54.7233C87.2646 54.7141 87.2363 54.8124 87.211 54.9417ZM87.0962 55.4599C87.0698 55.5775 87.0555 55.6813 87.0647 55.6906C87.0737 55.6995 87.1027 55.6105 87.1289 55.4926C87.1553 55.375 87.1696 55.2711 87.1604 55.2619C87.1514 55.253 87.1224 55.342 87.0962 55.4599ZM86.9244 56.1926C83.4884 70.4357 72.1635 82.3127 58.0845 86.4382C57.8471 86.5077 57.6606 86.5722 57.6698 86.5814C57.6981 86.6097 59.1968 86.139 60.1066 85.8158C73.414 81.0892 83.5025 69.9873 86.9394 56.287C87.0049 56.0262 87.053 55.796 87.0461 55.7757C87.0394 55.7555 86.9847 55.9431 86.9244 56.1926ZM5.63503 58.3196C5.63503 58.4948 6.36367 60.5768 6.84161 61.7681C11.4474 73.2445 20.9795 82.2121 32.6966 86.092C33.7891 86.4536 35.9115 87.0553 36.0714 87.0484C36.1031 87.0471 35.7841 86.9541 35.3625 86.8417C22.486 83.41 12.1608 74.4224 7.06402 62.2094C6.56722 61.0186 5.62905 58.3854 5.68264 58.3318C5.69207 58.3223 6.6123 58.6918 7.7278 59.1529C8.8433 59.6139 12.4256 61.0936 15.6886 62.4412L21.6212 64.8909L21.6601 65.0465C22.8278 69.6993 26.4137 73.079 31.0454 73.8918C31.2853 73.934 31.5268 73.9664 31.582 73.9639C31.6372 73.9616 31.5404 73.9374 31.3667 73.9102C26.7219 73.1833 22.972 69.8407 21.7964 65.3791C21.7222 65.0969 21.6564 64.8619 21.6502 64.8568C21.6442 64.852 21.0009 64.5847 20.2209 64.2631C19.4408 63.9413 17.569 63.169 16.0616 62.5468C14.5542 61.9246 12.3462 61.0133 11.155 60.5216C9.96386 60.0299 8.25312 59.3239 7.35359 58.9524C5.68747 58.2643 5.63503 58.245 5.63503 58.3196ZM42.0249 61.4997C42.0249 61.6157 42.0321 61.6631 42.0408 61.6051C42.0496 61.5471 42.0496 61.452 42.0408 61.394C42.0321 61.336 42.0249 61.3834 42.0249 61.4997ZM39.2652 61.7874C39.2654 61.914 39.2725 61.9612 39.281 61.8922C39.2898 61.8231 39.2896 61.7195 39.2808 61.662C39.2721 61.6044 39.2649 61.6608 39.2652 61.7874ZM42.0675 62.3629C42.0673 62.7215 42.073 62.8735 42.0801 62.7004C42.0873 62.5273 42.0873 62.2338 42.0804 62.0482C42.0732 61.8625 42.0675 62.004 42.0675 62.3629ZM44.6722 62.8808C44.6722 63.0811 44.6787 63.1633 44.6865 63.0629C44.6946 62.9628 44.6946 62.7986 44.6865 62.6985C44.6787 62.5984 44.6722 62.6803 44.6722 62.8808ZM39.2652 62.7848C39.2654 62.9114 39.2725 62.9586 39.281 62.8896C39.2898 62.8207 39.2896 62.7172 39.2808 62.6594C39.2721 62.6018 39.2649 62.6582 39.2652 62.7848ZM39.2196 63.1301C39.2088 63.3447 39.0635 63.9588 38.9264 64.3704C38.1138 66.8079 35.9656 68.5394 33.35 68.8649L33.0625 68.9006L33.3641 68.8833C35.4389 68.7643 37.5917 67.2738 38.6032 65.2564C38.9298 64.605 39.2944 63.3778 39.2424 63.1046C39.2284 63.0298 39.2245 63.0339 39.2196 63.1301ZM42.0022 63.3412C41.6319 67.3956 38.2258 70.8386 34.0895 71.3395C33.862 71.3671 33.7426 71.3915 33.8243 71.3938C34.2526 71.4058 35.4686 71.1392 36.1731 70.8788C38.6276 69.9721 40.6229 67.975 41.5327 65.5142C41.7989 64.7946 42.0792 63.4957 42.0445 63.1423C42.0376 63.0723 42.0185 63.1617 42.0022 63.3412ZM44.5922 63.6867C44.0466 68.8007 39.9457 73.0746 34.8066 73.8849C34.5433 73.9266 34.3733 73.9623 34.4292 73.9643C34.5879 73.9706 35.3798 73.8269 35.8734 73.7024C40.2303 72.6034 43.5526 69.1245 44.4639 64.7067C44.5869 64.1101 44.6732 63.4855 44.6525 63.3396C44.6451 63.2876 44.618 63.4439 44.5922 63.6867ZM24.8784 66.2773C24.8784 66.6161 25.9946 68.1854 26.7615 68.9243C28.1053 70.2193 30.1061 71.1935 31.7216 71.339C31.8654 71.3519 31.7936 71.3309 31.5291 71.2824C29.0332 70.8245 26.9372 69.4687 25.5687 67.4273C25.3074 67.0376 24.8941 66.3038 24.9219 66.2787C24.9295 66.2718 26.1175 66.7573 27.5616 67.3574C30.8394 68.7194 31.1929 68.841 31.9891 68.8801C32.1579 68.8884 32.2442 68.8886 32.1809 68.8803C30.8936 68.7141 31.2221 68.8306 26.4884 66.8611C24.7973 66.1574 24.8784 66.1868 24.8784 66.2773ZM32.5174 68.9323C32.6182 68.9402 32.7734 68.9402 32.8624 68.9321C32.9514 68.924 32.8689 68.9176 32.6791 68.9176C32.4894 68.9178 32.4167 68.9245 32.5174 68.9323ZM32.0567 71.3871C32.1152 71.3959 32.2014 71.3956 32.2486 71.3864C32.2957 71.3774 32.2479 71.3701 32.1425 71.3705C32.0372 71.3708 31.9986 71.3781 32.0567 71.3871ZM32.7099 71.4265C32.8953 71.4334 33.1886 71.4334 33.3615 71.4265C33.5345 71.4193 33.3827 71.4136 33.0241 71.4136C32.6658 71.4136 32.5243 71.4193 32.7099 71.4265ZM31.7886 73.9959C31.8468 74.0046 31.933 74.0044 31.9802 73.9954C32.0273 73.9862 31.9797 73.9791 31.8741 73.9793C31.7688 73.9795 31.7301 73.9871 31.7886 73.9959ZM34.0502 73.9959C34.1083 74.0046 34.1946 74.0044 34.2417 73.9954C34.2889 73.9862 34.2413 73.9791 34.1359 73.9793C34.0304 73.9795 33.992 73.9871 34.0502 73.9959ZM32.249 74.035C32.35 74.0428 32.5052 74.0428 32.594 74.0348C32.683 74.0267 32.6007 74.0203 32.4109 74.0203C32.2212 74.0205 32.1483 74.0269 32.249 74.035ZM33.4554 74.0348C33.5451 74.0428 33.6916 74.0428 33.7813 74.0348C33.871 74.0267 33.7976 74.02 33.6184 74.02C33.439 74.02 33.3659 74.0267 33.4554 74.0348ZM57.4425 86.6288C57.3942 86.6497 57.3871 86.6619 57.4235 86.6619C57.455 86.6619 57.5154 86.647 57.5575 86.6288C57.6058 86.6081 57.613 86.5959 57.5766 86.5959C57.5451 86.5959 57.4846 86.6108 57.4425 86.6288ZM57.0975 86.7262C56.9464 86.7787 56.9462 86.7791 57.0784 86.7566C57.2349 86.7303 57.3947 86.6719 57.3084 86.6725C57.2767 86.673 57.1819 86.6969 57.0975 86.7262ZM56.0241 86.9943C55.5814 87.1064 55.2451 87.1992 55.2766 87.2008C55.3449 87.2045 56.8928 86.8275 56.9195 86.8008C56.9561 86.7639 56.8146 86.7945 56.0241 86.9943ZM36.3209 87.1111C36.4263 87.1405 36.5383 87.1633 36.57 87.1619C36.6015 87.1603 36.5413 87.1352 36.4359 87.1058C36.3304 87.0763 36.2184 87.0535 36.1866 87.0551C36.1551 87.0565 36.2154 87.0818 36.3209 87.1111ZM36.896 87.2441C37.0434 87.2821 37.1986 87.3127 37.2409 87.3122C37.283 87.3118 37.1883 87.2795 37.03 87.2404C36.872 87.2011 36.7165 87.1704 36.685 87.1721C36.6533 87.1734 36.7483 87.2059 36.896 87.2441ZM54.97 87.2427C54.8941 87.2671 54.8902 87.2733 54.951 87.2733C54.993 87.2733 55.0708 87.2595 55.1235 87.2427C55.1993 87.2183 55.2033 87.2121 55.1425 87.2121C55.1004 87.2121 55.0227 87.2259 54.97 87.2427ZM54.2225 87.399C54.0429 87.4407 53.9873 87.4623 54.0885 87.451C54.2952 87.4278 54.715 87.3279 54.6059 87.3279C54.5636 87.3279 54.3911 87.3599 54.2225 87.399ZM37.8159 87.4345C37.9781 87.4759 38.1172 87.4759 37.9884 87.4345C37.9358 87.4177 37.8495 87.4041 37.7966 87.4045C37.7106 87.4052 37.7127 87.4082 37.8159 87.4345ZM53.6091 87.5114C53.5065 87.5374 53.5045 87.5406 53.59 87.5406C53.6427 87.5406 53.7377 87.5275 53.8009 87.5114C53.9035 87.4851 53.9056 87.4819 53.82 87.4819C53.7674 87.4819 53.6724 87.4952 53.6091 87.5114ZM52.9384 87.6292C52.7038 87.6746 52.6493 87.6925 52.785 87.6796C53.0431 87.655 53.5312 87.5588 53.3984 87.5588C53.3458 87.5588 53.1385 87.5903 52.9384 87.6292ZM38.8509 87.6264C38.9142 87.6426 39.0177 87.6557 39.0809 87.6557C39.1775 87.6559 39.1716 87.6509 39.0425 87.6242C38.8484 87.5839 38.6916 87.5857 38.8509 87.6264ZM39.5791 87.7436C39.6635 87.7611 39.7758 87.7745 39.8284 87.7733C39.8899 87.7717 39.8692 87.7597 39.771 87.7393C39.6865 87.7218 39.5745 87.7086 39.5216 87.7098C39.4602 87.7112 39.4809 87.7234 39.5791 87.7436ZM52.1429 87.768C52.1799 87.7775 52.2402 87.7775 52.2772 87.768C52.314 87.7584 52.2839 87.7503 52.21 87.7503C52.1362 87.7503 52.1061 87.7584 52.1429 87.768ZM40.0108 87.8069C40.0587 87.8161 40.1277 87.8159 40.1642 87.8062C40.2008 87.7966 40.1619 87.7892 40.0775 87.7894C39.9931 87.7899 39.9632 87.7977 40.0108 87.8069ZM51.8747 87.8062C51.9115 87.8159 51.972 87.8159 52.0088 87.8062C52.0456 87.7966 52.0155 87.7887 51.9416 87.7887C51.8678 87.7887 51.8377 87.7966 51.8747 87.8062ZM40.2979 87.8447C40.3349 87.8543 40.3952 87.8543 40.4322 87.8447C40.469 87.835 40.4389 87.8272 40.365 87.8272C40.2912 87.8272 40.2611 87.835 40.2979 87.8447ZM51.6063 87.8447C51.6431 87.8543 51.7036 87.8543 51.7404 87.8447C51.7774 87.835 51.7471 87.8272 51.6735 87.8272C51.5996 87.8272 51.5695 87.835 51.6063 87.8447ZM40.5858 87.8838C40.6337 87.8928 40.7027 87.8926 40.7392 87.8829C40.7758 87.8732 40.7369 87.8659 40.6525 87.8663C40.5681 87.8668 40.5382 87.8746 40.5858 87.8838ZM51.2808 87.8838C51.3287 87.8928 51.3977 87.8926 51.4342 87.8829C51.4708 87.8732 51.4319 87.8659 51.3475 87.8663C51.2631 87.8668 51.2332 87.8746 51.2808 87.8838ZM40.8927 87.922C40.9403 87.9312 41.0093 87.931 41.0458 87.9213C41.0826 87.9117 41.0435 87.9043 40.9591 87.9045C40.8749 87.905 40.8448 87.9128 40.8927 87.922ZM50.9742 87.922C51.0219 87.9312 51.0909 87.931 51.1277 87.9213C51.1642 87.9117 51.1251 87.9043 51.041 87.9045C50.9565 87.905 50.9266 87.9128 50.9742 87.922ZM41.2377 87.9605C41.2853 87.9697 41.3543 87.9692 41.3908 87.9598C41.4276 87.9501 41.3885 87.9425 41.3041 87.943C41.2199 87.9434 41.1898 87.9513 41.2377 87.9605ZM50.6677 87.9605C50.7153 87.9697 50.7843 87.9692 50.8208 87.9598C50.8576 87.9501 50.8185 87.9425 50.7341 87.943C50.6499 87.9434 50.6198 87.9513 50.6677 87.9605ZM41.5822 87.9987C41.6296 88.0079 41.7071 88.0079 41.7547 87.9987C41.8021 87.9897 41.7632 87.9821 41.6684 87.9821C41.5735 87.9821 41.5346 87.9897 41.5822 87.9987ZM50.2652 87.9991C50.3233 88.0081 50.4096 88.0079 50.4567 87.9987C50.5039 87.9895 50.4563 87.9823 50.3509 87.9826C50.2454 87.983 50.207 87.9904 50.2652 87.9991ZM41.9852 88.0376C42.0434 88.0463 42.1296 88.0461 42.1767 88.0371C42.2239 88.0279 42.1763 88.0208 42.0709 88.021C41.9654 88.0212 41.927 88.0288 41.9852 88.0376ZM49.8817 88.0376C49.9402 88.0463 50.0264 88.0461 50.0736 88.0371C50.1207 88.0279 50.0729 88.0208 49.9675 88.021C49.8622 88.0212 49.8236 88.0288 49.8817 88.0376ZM42.3679 88.0758C42.4258 88.0848 42.5208 88.0848 42.5788 88.0758C42.6367 88.067 42.5894 88.0599 42.4734 88.0599C42.3573 88.0599 42.3099 88.067 42.3679 88.0758ZM49.4213 88.0758C49.4792 88.0848 49.5742 88.0848 49.6322 88.0758C49.6902 88.067 49.6428 88.0599 49.5266 88.0599C49.4107 88.0599 49.3633 88.067 49.4213 88.0758ZM42.8861 88.1145C42.9551 88.1232 43.0586 88.123 43.1161 88.1142C43.1738 88.1055 43.1175 88.0983 42.9909 88.0986C42.8644 88.0988 42.8173 88.1059 42.8861 88.1145ZM48.9045 88.1145C48.9735 88.1232 49.077 88.123 49.1345 88.1142C49.192 88.1055 49.1356 88.0983 49.0091 88.0986C48.8826 88.0988 48.8355 88.1059 48.9045 88.1145ZM43.4615 88.1534C43.5517 88.1614 43.6897 88.1612 43.7681 88.1529C43.8465 88.1448 43.7729 88.1382 43.6041 88.1382C43.4355 88.1384 43.3714 88.1451 43.4615 88.1534ZM48.2513 88.1529C48.3194 88.1612 48.4401 88.1614 48.5197 88.1531C48.599 88.1448 48.5434 88.1379 48.396 88.1377C48.2483 88.1375 48.1832 88.1444 48.2513 88.1529ZM44.2861 88.1918C44.408 88.1996 44.5977 88.1994 44.7077 88.1918C44.8176 88.184 44.7178 88.1778 44.486 88.1778C44.2539 88.178 44.1639 88.1842 44.2861 88.1918ZM47.3322 88.1918C47.4428 88.1996 47.6238 88.1996 47.7347 88.1918C47.8453 88.184 47.7547 88.1778 47.5335 88.1778C47.312 88.1778 47.2213 88.184 47.3322 88.1918ZM45.8163 88.2302C45.9577 88.2376 46.1992 88.2376 46.3529 88.2305C46.5067 88.2231 46.391 88.2171 46.096 88.2169C45.8006 88.2169 45.6748 88.2229 45.8163 88.2302Z M45.2242 3.82538C45.2718 3.83458 45.3408 3.83412 45.3776 3.82469C45.4142 3.81502 45.3751 3.80742 45.2909 3.80788C45.2065 3.80834 45.1766 3.81617 45.2242 3.82538ZM46.6426 3.82538C46.6902 3.83458 46.7592 3.83412 46.7958 3.82469C46.8326 3.81502 46.7935 3.80742 46.7091 3.80788C46.6249 3.80834 46.5948 3.81617 46.6426 3.82538ZM75.2484 15.634C75.32 15.7076 75.3871 15.7682 75.3975 15.7682C75.408 15.7682 75.3581 15.7076 75.2866 15.634C75.2151 15.5601 75.1479 15.4996 75.1376 15.4996C75.127 15.4996 75.1769 15.5601 75.2484 15.634ZM16.5768 15.7779L16.4641 15.9024L16.5888 15.7898C16.7049 15.6849 16.7309 15.6531 16.7015 15.6531C16.6948 15.6531 16.6387 15.7093 16.5768 15.7779ZM15.8882 16.4684L15.6209 16.7465L15.8988 16.479C16.1568 16.2304 16.1941 16.1901 16.166 16.1901C16.1603 16.1901 16.0352 16.3154 15.8882 16.4684ZM76.2066 16.5858C76.2066 16.5925 76.2627 16.6487 76.3313 16.7106L76.4559 16.8232L76.3432 16.6986C76.2383 16.5824 76.2066 16.5561 76.2066 16.5858ZM54.1818 29.7427L54.0691 29.8674L54.1938 29.7546C54.3099 29.6497 54.3359 29.6181 54.3065 29.6181C54.2998 29.6181 54.2437 29.6741 54.1818 29.7427ZM43.8261 35.392C43.8263 35.4975 43.8336 35.5361 43.8426 35.4779C43.8514 35.4194 43.8511 35.3331 43.8419 35.2859C43.833 35.2387 43.8256 35.2866 43.8261 35.392ZM88.1388 45.2327C88.1388 45.3276 88.1464 45.3665 88.1553 45.3191C88.1645 45.2716 88.1645 45.1938 88.1553 45.1464C88.1464 45.099 88.1388 45.1379 88.1388 45.2327ZM59.7147 46.1434C59.8048 46.1514 59.9428 46.1512 60.0215 46.1429C60.0999 46.1349 60.0261 46.1282 59.8575 46.1282C59.6889 46.1284 59.6245 46.1351 59.7147 46.1434ZM88.1388 46.7674C88.1388 46.8623 88.1464 46.9012 88.1553 46.8538C88.1645 46.8061 88.1645 46.7285 88.1553 46.6811C88.1464 46.6335 88.1388 46.6724 88.1388 46.7674ZM33.0147 53.3159C33.0515 53.3256 33.112 53.3256 33.1488 53.3159C33.1856 53.3063 33.1554 53.2984 33.0816 53.2984C33.0078 53.2984 32.9776 53.3063 33.0147 53.3159ZM44.7086 62.3629C44.7088 62.4895 44.7159 62.5367 44.7244 62.4677C44.7329 62.3986 44.7327 62.295 44.724 62.2375C44.7152 62.1799 44.7083 62.2363 44.7086 62.3629ZM32.8624 74.0732C32.9632 74.0813 33.1184 74.0811 33.2074 74.073C33.2964 74.0652 33.2139 74.0585 33.0241 74.0587C32.8344 74.0587 32.7617 74.0654 32.8624 74.0732ZM15.755 75.407C15.8696 75.523 15.9719 75.6181 15.9825 75.6181C15.9928 75.6181 15.908 75.523 15.7934 75.407C15.6789 75.291 15.5765 75.1961 15.566 75.1961C15.5554 75.1961 15.6405 75.291 15.755 75.407ZM76.3002 75.3207L76.1875 75.4454L76.3122 75.3326C76.4283 75.2277 76.4543 75.1961 76.4246 75.1961C76.418 75.1961 76.3621 75.2521 76.3002 75.3207ZM16.6175 76.2702C16.7212 76.3759 16.8149 76.4622 16.8254 76.4622C16.836 76.4622 16.7597 76.3759 16.6559 76.2702C16.552 76.1648 16.4584 76.0785 16.448 76.0785C16.4374 76.0785 16.5138 76.1648 16.6175 76.2702ZM75.2845 76.3374L75.1525 76.4813L75.2963 76.3492C75.3754 76.2764 75.44 76.2115 75.44 76.2053C75.44 76.1758 75.4076 76.2034 75.2845 76.3374ZM45.2228 88.1911C45.2909 88.1997 45.4117 88.1999 45.4913 88.1916C45.5708 88.1833 45.5152 88.1762 45.3675 88.1762C45.2199 88.1759 45.1548 88.1826 45.2228 88.1911ZM46.6426 88.1907C46.6902 88.1999 46.7592 88.1994 46.7958 88.19C46.8326 88.1803 46.7935 88.1727 46.7091 88.1732C46.6249 88.1736 46.5948 88.1815 46.6426 88.1907Z", 
        new Rect(0, 0, 92, 92)
    ));    
    
    // Collections size 
    public static readonly IconValue Size = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M3.67188 8.90463L5.66927 4.91563C5.78138 4.69001 5.9542 4.50015 6.16831 4.36738C6.38242 4.23461 6.62932 4.1642 6.88125 4.16406H13.3271C13.579 4.1642 13.8259 4.23461 14.04 4.36738C14.2541 4.50015 14.427 4.69001 14.5391 4.91563L16.5365 8.90463H3.67188ZM3.33008 13.6416V9.5791H16.8717V13.6416C16.8717 14.0007 16.7291 14.3452 16.4751 14.5991C16.2212 14.8531 15.8767 14.9958 15.5176 14.9958H4.68424C4.3251 14.9958 3.98066 14.8531 3.7267 14.5991C3.47275 14.3452 3.33008 14.0007 3.33008 13.6416ZM6.19466 11.6104C5.73442 11.6104 5.36133 11.9834 5.36133 12.4437C5.36133 12.9039 5.73442 13.277 6.19466 13.277H6.203C6.66323 13.277 7.03633 12.9039 7.03633 12.4437C7.03633 11.9834 6.66323 11.6104 6.203 11.6104H6.19466ZM8.06641 12.4437C8.06641 11.9834 8.4395 11.6104 8.89974 11.6104H8.90807C9.36831 11.6104 9.74141 11.9834 9.74141 12.4437C9.74141 12.9039 9.36831 13.277 8.90807 13.277H8.89974C8.4395 13.277 8.06641 12.9039 8.06641 12.4437Z", 
        new Rect(0, 0, 20, 20)
    ));
    
    // new gamepad icon 
    // https://fonts.google.com/icons?selected=Material+Symbols+Outlined:sports_esports:FILL@0;wght@400;GRAD@0;opsz@24&icon.query=gamepad&icon.size=24&icon.color=%23e8eaed
    public static readonly IconValue GamepadOutline = new SimpleVectorIcon(new SimpleVectorIconImage(
"M182-200q-51 0-79-35.5T82-322l42-300q9-60 53.5-99T282-760h396q60 0 104.5 39t53.5 99l42 300q7 51-21 86.5T778-200q-21 0-39-7.5T706-230l-90-90H344l-90 90q-15 15-33 22.5t-39 7.5Zm16-86 114-114h336l114 114q2 2 16 6 11 0 17.5-6.5T800-304l-44-308q-4-29-26-48.5T678-680H282q-30 0-52 19.5T204-612l-44 308q-2 11 4.5 17.5T182-280q2 0 16-6Zm482-154q17 0 28.5-11.5T720-480q0-17-11.5-28.5T680-520q-17 0-28.5 11.5T640-480q0 17 11.5 28.5T680-440Zm-80-120q17 0 28.5-11.5T640-600q0-17-11.5-28.5T600-640q-17 0-28.5 11.5T560-600q0 17 11.5 28.5T600-560ZM310-440h60v-70h70v-60h-70v-70h-60v70h-70v60h70v70Zm170-40Z",
        new Rect(0, -960, 960, 960 )
    ));
    
    // new health check icon 
    // https://fonts.google.com/icons?icon.query=cardio&icon.size=34&icon.color=%23e8eaed&selected=Material+Symbols+Outlined:cardiology:FILL@0;wght@400;GRAD@0;opsz@40
    public static readonly IconValue Cardiology = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M293.33-840q55.34 0 103.34 25.33 48 25.34 83.33 72.67 39.33-49.33 86.33-73.67 47-24.33 100.34-24.33 90.66 0 152 61.33Q880-717.33 880-626q0 11.67-1.17 23-1.16 11.33-3.83 23h-68q3.67-11.67 5-23t1.33-23q0-64-41.33-105.67-41.33-41.66-105.33-41.66-49.67 0-92.34 29.83-42.66 29.83-65.66 81.5h-58q-22.34-51-65-81.17-42.67-30.16-92.34-30.16-64 0-105.33 41.66Q146.67-690 146.67-626q0 11.67 1.33 23t5 23H85q-2.67-11.67-3.83-23Q80-614.33 80-626q0-91.33 61.33-152.67 61.34-61.33 152-61.33Zm-94.66 460H288q36 38.33 83 83.33t109 101.34q62-56.34 108.67-101.34 46.66-45 82.66-83.33h90.34q-40.67 46.67-98 102.67-57.34 56-137 128.66l-46.67 42-46.67-42q-79.66-72.66-136.83-128.66-57.17-56-97.83-102.67ZM442-326.67q10.33 0 18.17-6.16Q468-339 471.33-349L530-524l42.33 62.67q5 6.66 12 10.66t15.67 4h313.33v-66.66H619L548-618q-5.33-7.67-13.17-11.17-7.83-3.5-16.83-3.5-10.33 0-18.5 6.17T488-610.33l-58 174.66-42.67-63q-5-6.66-11.66-10.66-6.67-4-15.34-4H46.67v66.66h293.66l71 105.34q5.34 7.66 13.5 11.16 8.17 3.5 17.17 3.5Zm38-157.66Z",
        new Rect(0, -960, 960, 960 )
    ));
    
    // new library icon 
    // https://fonts.google.com/icons?selected=Material+Symbols+Outlined:sports_esports:FILL@0;wght@400;GRAD@0;opsz@24&icon.query=gamepad&icon.size=24&icon.color=%23e8eaed
    public static readonly IconValue LibraryOutline = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M5.53149 14C6.18913 14 6.8292 14.0833 7.4517 14.25C8.07434 14.4167 8.68427 14.625 9.28149 14.875V5.41667C8.69816 5.11111 8.09399 4.88194 7.46899 4.72917C6.84399 4.57639 6.19663 4.5 5.52691 4.5C5.01386 4.5 4.50441 4.54514 3.99858 4.63542C3.49274 4.72569 3.00372 4.875 2.53149 5.08333V14.5C3.01761 14.3194 3.50934 14.191 4.0067 14.1146C4.5042 14.0382 5.01247 14 5.53149 14ZM10.7815 14.875C11.3787 14.5972 11.9886 14.3819 12.6113 14.2292C13.2338 14.0764 13.8739 14 14.5315 14C15.0454 14 15.5558 14.0312 16.0627 14.0938C16.5697 14.1562 17.0593 14.2917 17.5315 14.5V5.08333C17.0454 4.90278 16.551 4.76042 16.0484 4.65625C15.5457 4.55208 15.0395 4.5 14.5298 4.5C13.8643 4.5 13.219 4.57639 12.594 4.72917C11.969 4.88194 11.3648 5.11111 10.7815 5.41667V14.875ZM10.0315 17C9.35094 16.5556 8.63566 16.1944 7.88566 15.9167C7.13566 15.6389 6.35094 15.5 5.53149 15.5C5.00372 15.5 4.47594 15.5521 3.94816 15.6562C3.42038 15.7604 2.91344 15.9167 2.42733 16.125C2.09399 16.2639 1.77802 16.2394 1.47941 16.0515C1.1808 15.8635 1.03149 15.5894 1.03149 15.2292V4.75C1.03149 4.55556 1.08358 4.375 1.18774 4.20833C1.29191 4.04167 1.43427 3.91667 1.61483 3.83333C2.23983 3.55556 2.87899 3.34722 3.53233 3.20833C4.18566 3.06944 4.85205 3 5.53149 3C6.32247 3 7.09636 3.09375 7.85316 3.28125C8.60997 3.46875 9.33608 3.75 10.0315 4.125C10.7398 3.76389 11.4703 3.48611 12.223 3.29167C12.9755 3.09722 13.745 3 14.5315 3C15.2109 3 15.8773 3.06944 16.5307 3.20833C17.184 3.34722 17.8232 3.55556 18.4482 3.83333C18.6287 3.91667 18.7745 4.04167 18.8857 4.20833C18.9968 4.375 19.0523 4.55556 19.0523 4.75V15.2292C19.0523 15.5764 18.9447 15.8542 18.7294 16.0625C18.5141 16.2708 18.2884 16.3194 18.0523 16.2083C17.4968 15.9583 16.924 15.7778 16.334 15.6667C15.7441 15.5556 15.1433 15.5 14.5315 15.5C13.7121 15.5 12.9273 15.6389 12.1773 15.9167C11.4273 16.1944 10.7121 16.5556 10.0315 17Z",
        new Rect(0, 0, 21, 20 )
    ));
    
    // new collections icon 
    public static readonly IconValue CollectionsOutline = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M11.9911 15.7433L5.44 10.9822L4 12.0291L12 17.8455L20 12.0291L18.5511 10.9739L11.9911 15.7433ZM11.9911 19.8977L5.44 15.1366L4 16.1835L12 21.9999L20 16.1835L18.5511 15.1283L11.9911 19.8977ZM19.9992 7.81605L18.5415 8.87125L11.9996 13.6321L5.44881 8.87125L4 7.81605L11.9996 2L19.9992 7.81605ZM16.2656 7.81405L11.9996 4.71247L7.72972 7.81685L11.9986 10.9193L16.2656 7.81405Z",
        new Rect(0, 0, 24, 24 )
    ));
    
    // new mods icon
    public static readonly IconValue ModsOutline = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M18.97 12.4328L17.6972 13.4184L11.985 17.8656L6.26506 13.4184L5 12.4328L11.985 7L18.97 12.4328ZM15.71 12.4309L11.985 9.53372L8.25668 12.4335L11.9842 15.3316L15.71 12.4309Z",
        new Rect(0, 0, 24, 24 )
    ));
    
    // GitHub icon
    public static readonly IconValue GitHub = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M48.854 0C21.839 0 0 22 0 49.217c0 21.756 13.993 40.172 33.405 46.69 2.427.49 3.316-1.059 3.316-2.362 0-1.141-.08-5.052-.08-9.127-13.59 2.934-16.42-5.867-16.42-5.867-2.184-5.704-5.42-7.17-5.42-7.17-4.448-3.015.324-3.015.324-3.015 4.934.326 7.523 5.052 7.523 5.052 4.367 7.496 11.404 5.378 14.235 4.074.404-3.178 1.699-5.378 3.074-6.6-10.839-1.141-22.243-5.378-22.243-24.283 0-5.378 1.94-9.778 5.014-13.2-.485-1.222-2.184-6.275.486-13.038 0 0 4.125-1.304 13.426 5.052a46.97 46.97 0 0 1 12.214-1.63c4.125 0 8.33.571 12.213 1.63 9.302-6.356 13.427-5.052 13.427-5.052 2.67 6.763.97 11.816.485 13.038 3.155 3.422 5.015 7.822 5.015 13.2 0 18.905-11.404 23.06-22.324 24.283 1.78 1.548 3.316 4.481 3.316 9.126 0 6.6-.08 11.897-.08 13.526 0 1.304.89 2.853 3.316 2.364 19.412-6.52 33.405-24.935 33.405-46.691C97.707 22 75.788 0 48.854 0z",
        new Rect(0, 0, 98, 96 )
    ));
    
    // Premium icon
    public static readonly IconValue Premium = new SimpleVectorIcon(new SimpleVectorIconImage(
            "M7.99998 14L1.33331 6L3.33331 2H12.6666L14.6666 6L7.99998 14ZM6.41665 5.33333H9.58331L8.58331 3.33333H7.41665L6.41665 5.33333ZM7.33331 11.1167V6.66667H3.63331L7.33331 11.1167ZM8.66665 11.1167L12.3666 6.66667H8.66665V11.1167ZM11.0666 5.33333H12.8333L11.8333 3.33333H10.0666L11.0666 5.33333ZM3.16665 5.33333H4.93331L5.93331 3.33333H4.16665L3.16665 5.33333Z",
            new Rect(0, 0, 16, 16 )
    ));
    
    
    // Toggle On icon
    public static readonly IconValue ToggleOn = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M7 7C4.23858 7 2 9.23858 2 12C2 14.7614 4.23858 17 7 17H17C19.7614 17 22 14.7614 22 12C22 9.23858 19.7614 7 17 7H7ZM17 15.3329C18.841 15.3329 20.3333 13.8405 20.3333 11.9996C20.3333 10.1586 18.841 8.66626 17 8.66626C15.1591 8.66626 13.6667 10.1586 13.6667 11.9996C13.6667 13.8405 15.1591 15.3329 17 15.3329Z",
        new Rect(0, 0, 24, 24 )
    ));
    
    // Toggle Off icon
    public static readonly IconValue ToggleOff = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M7 8H17C19.2091 8 21 9.79086 21 12C21 14.2091 19.2091 16 17 16H7C4.79086 16 3 14.2091 3 12C3 9.79086 4.79086 8 7 8ZM2 12C2 9.23858 4.23858 7 7 7H17C19.7614 7 22 9.23858 22 12C22 14.7614 19.7614 17 17 17H7C4.23858 17 2 14.7614 2 12ZM7 14.5C8.38071 14.5 9.5 13.3807 9.5 12C9.5 10.6193 8.38071 9.5 7 9.5C5.61929 9.5 4.5 10.6193 4.5 12C4.5 13.3807 5.61929 14.5 7 14.5Z",
        new Rect(0, 0, 24, 24 )
    ));
    
    // Toggle Indeterminate icon
    public static readonly IconValue ToggleIndeterminate = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M19 12C19 13.1046 18.1046 14 17 14C15.8954 14 15 13.1046 15 12C15 10.8954 15.8954 10 17 10C18.1046 10 19 10.8954 19 12Z M7 7C4.23858 7 2 9.23858 2 12C2 14.7614 4.23858 17 7 17H17C19.7614 17 22 14.7614 22 12C22 9.23858 19.7614 7 17 7H7ZM21 12C21 14.2091 19.2091 16 17 16C14.7909 16 13 14.2091 13 12C13 9.79086 14.7909 8 17 8C19.2091 8 21 9.79086 21 12Z",
        new Rect(0, 0, 24, 24 )
    ));
    
    // vertical drag handle
    public static readonly IconValue DragVerticalDots = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M1.2 0.5C0.54 0.5 0 1.00625 0 1.625C0 2.24375 0.54 2.75 1.2 2.75C1.86 2.75 2.4 2.24375 2.4 1.625C2.4 1.00625 1.86 0.5 1.2 0.5ZM0 5C0 4.38125 0.54 3.875 1.2 3.875C1.86 3.875 2.4 4.38125 2.4 5C2.4 5.61875 1.86 6.125 1.2 6.125C0.54 6.125 0 5.61875 0 5ZM1.2 9.5C1.86 9.5 2.4 8.99375 2.4 8.375C2.4 7.75625 1.86 7.25 1.2 7.25C0.54 7.25 0 7.75625 0 8.375C0 8.99375 0.54 9.5 1.2 9.5ZM6 1.625C6 2.24375 5.46 2.75 4.8 2.75C4.14 2.75 3.6 2.24375 3.6 1.625C3.6 1.00625 4.14 0.5 4.8 0.5C5.46 0.5 6 1.00625 6 1.625ZM4.8 3.875C4.14 3.875 3.6 4.38125 3.6 5C3.6 5.61875 4.14 6.125 4.8 6.125C5.46 6.125 6 5.61875 6 5C6 4.38125 5.46 3.875 4.8 3.875ZM3.6 8.375C3.6 7.75625 4.14 7.25 4.8 7.25C5.46 7.25 6 7.75625 6 8.375C6 8.99375 5.46 9.5 4.8 9.5C4.14 9.5 3.6 8.99375 3.6 8.375Z",
        new Rect(0, 0, 6, 10 )
    ));
    
    // collection visibility listed
    public static readonly IconValue VisibilityListed = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M7.99967 14.6666C7.07745 14.6666 6.21079 14.4916 5.39967 14.1416C4.58856 13.7916 3.88301 13.3166 3.28301 12.7166C2.68301 12.1166 2.20801 11.411 1.85801 10.5999C1.50801 9.78881 1.33301 8.92214 1.33301 7.99992C1.33301 7.0777 1.50801 6.21103 1.85801 5.39992C2.20801 4.58881 2.68301 3.88325 3.28301 3.28325C3.88301 2.68325 4.58856 2.20825 5.39967 1.85825C6.21079 1.50825 7.07745 1.33325 7.99967 1.33325C9.6219 1.33325 11.0413 1.84159 12.258 2.85825C13.4747 3.87492 14.233 5.14992 14.533 6.68325H13.1663C12.9552 5.87214 12.5747 5.14714 12.0247 4.50825C11.4747 3.86936 10.7997 3.38881 9.99967 3.06659V3.33325C9.99967 3.69992 9.86912 4.01381 9.60801 4.27492C9.3469 4.53603 9.03301 4.66659 8.66634 4.66659H7.33301V5.99992C7.33301 6.18881 7.26912 6.34714 7.14134 6.47492C7.01356 6.6027 6.85523 6.66659 6.66634 6.66659H5.33301V7.99992H6.66634V9.99992H5.99967L2.79967 6.79992C2.76634 6.99992 2.73579 7.19992 2.70801 7.39992C2.68023 7.59992 2.66634 7.79992 2.66634 7.99992C2.66634 9.45547 3.17745 10.7055 4.19967 11.7499C5.2219 12.7944 6.48856 13.3221 7.99967 13.3333V14.6666ZM14.0663 14.3333L11.933 12.1999C11.6997 12.3333 11.4497 12.4444 11.183 12.5333C10.9163 12.6221 10.633 12.6666 10.333 12.6666C9.49967 12.6666 8.79134 12.3749 8.20801 11.7916C7.62467 11.2083 7.33301 10.4999 7.33301 9.66659C7.33301 8.83325 7.62467 8.12492 8.20801 7.54159C8.79134 6.95825 9.49967 6.66659 10.333 6.66659C11.1663 6.66659 11.8747 6.95825 12.458 7.54159C13.0413 8.12492 13.333 8.83325 13.333 9.66659C13.333 9.96659 13.2886 10.2499 13.1997 10.5166C13.1108 10.7833 12.9997 11.0333 12.8663 11.2666L14.9997 13.3999L14.0663 14.3333ZM10.333 11.3333C10.7997 11.3333 11.1941 11.1721 11.5163 10.8499C11.8386 10.5277 11.9997 10.1333 11.9997 9.66659C11.9997 9.19992 11.8386 8.80547 11.5163 8.48325C11.1941 8.16103 10.7997 7.99992 10.333 7.99992C9.86634 7.99992 9.4719 8.16103 9.14967 8.48325C8.82745 8.80547 8.66634 9.19992 8.66634 9.66659C8.66634 10.1333 8.82745 10.5277 9.14967 10.8499C9.4719 11.1721 9.86634 11.3333 10.333 11.3333Z",
        new Rect(0, 0, 16, 16 )
    ));
    
    // collection visibility listed
    public static readonly IconValue VisibilityUnlisted = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M14.667 2.66675V2.33341C14.667 1.41341 13.9203 0.666748 13.0003 0.666748C12.0803 0.666748 11.3337 1.41341 11.3337 2.33341V2.66675C10.967 2.66675 10.667 2.96675 10.667 3.33341V6.00008C10.667 6.36675 10.967 6.66675 11.3337 6.66675H14.667C15.0337 6.66675 15.3337 6.36675 15.3337 6.00008V3.33341C15.3337 2.96675 15.0337 2.66675 14.667 2.66675ZM12.667 8.66675C12.667 8.44008 12.6403 8.22008 12.6137 8.00008H13.967C13.987 8.22008 14.0003 8.44008 14.0003 8.66675C14.0003 12.3467 11.0137 15.3334 7.33366 15.3334C3.65366 15.3334 0.666992 12.3467 0.666992 8.66675C0.666992 4.98675 3.65366 2.00008 7.33366 2.00008C8.03366 2.00008 8.70033 2.10675 9.33366 2.30675V4.00008C9.33366 4.73341 8.73366 5.33341 8.00033 5.33341H6.66699V6.66675C6.66699 7.03341 6.36699 7.33341 6.00033 7.33341H4.66699V8.66675H8.66699C9.03366 8.66675 9.33366 8.96675 9.33366 9.33341V11.3334H10.0003C10.6003 11.3334 11.0937 11.7201 11.267 12.2601C12.1337 11.3134 12.667 10.0534 12.667 8.66675ZM2.00033 8.66675C2.00033 11.3867 4.03366 13.6267 6.66699 13.9534V12.6667C5.93366 12.6667 5.33366 12.0667 5.33366 11.3334V10.6667L2.14033 7.47341C2.05366 7.86008 2.00033 8.25341 2.00033 8.66675ZM12.0003 2.66675H14.0003V2.33341C14.0003 1.78008 13.5537 1.33341 13.0003 1.33341C12.447 1.33341 12.0003 1.78008 12.0003 2.33341V2.66675Z",
        new Rect(0, 0, 16, 16 )
    ));
    
    public static readonly IconValue AvatarTest = new AvaloniaImage(new Bitmap(AssetLoader.Open(new Uri("avares://NexusMods.App.UI/Assets/DesignTime/cyberpunk_game.png"))));

#endregion

#region Panel Layout Icons
    // new mods icon
    public static readonly IconValue PanelAllFull = new SimpleVectorIcon(new SimpleVectorIconImage(
        "M9.30794 3.01852C9.70794 3.01852 10.0579 3.16852 10.3579 3.46852C10.6579 3.76851 10.8079 4.11852 10.8079 4.51852L10.795 9.5107C10.795 9.9107 10.645 10.2607 10.345 10.5607C10.045 10.8607 9.69502 11.0107 9.29502 11.0107H4.04503C3.64503 11.0107 3.29503 10.8607 2.99503 10.5607C2.69503 10.2607 2.54503 9.9107 2.54503 9.5107L2.55795 4.51852C2.55795 4.11852 2.70795 3.76851 3.00795 3.46852C3.30795 3.16852 3.65795 3.01852 4.05795 3.01852H9.30794Z M10.8143 19.2468C10.8143 20.2133 10.0308 20.9968 9.06428 20.9968H4.28861C3.32211 20.9968 2.53861 20.2133 2.53861 19.2468V14.7683C2.53861 13.8018 3.32211 13.0183 4.28861 13.0183L9.06428 13.0183C10.0308 13.0183 10.8143 13.8018 10.8143 14.7683V19.2468Z M21.0194 9.2607C21.0194 10.2272 20.2359 11.0107 19.2694 11.0107H14.4937C13.5272 11.0107 12.7437 10.2272 12.7437 9.2607V4.78219C12.7437 3.81569 13.5272 3.03219 14.4937 3.03219L19.2694 3.03219C20.2359 3.03219 21.0194 3.81569 21.0194 4.78219L21.0194 9.2607Z M21.0194 19.2501C21.0194 20.2166 20.2359 21.0001 19.2694 21.0001H14.4937C13.5272 21.0001 12.7437 20.2166 12.7437 19.2501V14.7716C12.7437 13.8051 13.5272 13.0216 14.4937 13.0216L19.2694 13.0216C20.2359 13.0216 21.0194 13.8051 21.0194 14.7716V19.2501Z",
        new Rect(0, 0, 24, 24 )
    ));
    #endregion
    
#region Brand Pictograms
    
    /// <summary>
    /// Brand pictogram for Health
    /// </summary>
    public static readonly IconValue NexusColor = new AvaloniaSvg("avares://NexusMods.App.UI/Assets/nexus-logo.svg");
    
    /// <summary>
    /// Brand pictogram for Health
    /// </summary>
    public static readonly IconValue PictogramHealth = new AvaloniaSvg("avares://NexusMods.App.UI/Assets/Pictograms/health.svg");
    
    /// <summary>
    /// Brand pictogram for Games in 3D
    /// </summary>
    public static readonly IconValue PictogramGame3D = new AvaloniaSvg("avares://NexusMods.App.UI/Assets/Pictograms/game-3d.svg");
    
    /// <summary>
    /// Brand pictogram for Loadouts
    /// </summary>
    public static readonly IconValue PictogramBox2 = new AvaloniaSvg("avares://NexusMods.App.UI/Assets/Pictograms/box2.svg");
    
    /// <summary>
    /// Brand pictogram for Settings
    /// </summary>
    public static readonly IconValue PictogramSettings = new AvaloniaSvg("avares://NexusMods.App.UI/Assets/Pictograms/settings.svg");

    /// <summary>
    /// Brand pictogram for Celebrate
    /// </summary>
    public static readonly IconValue PictogramCelebrate = new AvaloniaSvg("avares://NexusMods.App.UI/Assets/Pictograms/celebrate.svg");
    
    /// <summary>
    /// Brand pictogram for Playlist Add
    /// </summary>
    public static readonly IconValue PictogramPlaylistAdd = new AvaloniaSvg("avares://NexusMods.App.UI/Assets/Pictograms/playlist-add.svg");
    
    /// <summary>
    /// Brand pictogram for Collection in 3D
    /// </summary>
    public static readonly IconValue PictogramCollection3D = new AvaloniaSvg("avares://NexusMods.App.UI/Assets/Pictograms/collection-3d.svg");
    
    /// <summary>
    /// Brand pictogram for Library
    /// </summary>
    public static readonly IconValue PictogramLibrary = new AvaloniaSvg("avares://NexusMods.App.UI/Assets/Pictograms/library.svg");
    
    /// <summary>
    /// Brand pictogram for Upload
    /// </summary>
    public static readonly IconValue PictogramUpload = new AvaloniaSvg("avares://NexusMods.App.UI/Assets/Pictograms/upload.svg");
    
    /// <summary>
    /// Brand pictogram for Library
    /// </summary>
    public static readonly IconValue PictogramPremium = new AvaloniaSvg("avares://NexusMods.App.UI/Assets/Pictograms/premium.svg");

    /// <summary>
    /// Brand pictogram for Success
    /// </summary>
    public static readonly IconValue PictogramSuccess = new AvaloniaSvg("avares://NexusMods.App.UI/Assets/Pictograms/success.svg");
    
    /// <summary>
    /// Brand pictogram for Download
    /// </summary>
    public static readonly IconValue PictogramDownload = new AvaloniaSvg("avares://NexusMods.App.UI/Assets/Pictograms/download.svg");


#endregion
    
}
