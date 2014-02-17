
$(document).ready(function () {

    var viewModel = {};
    viewModel.MainViewModel = new MainViewModel();
    viewModel.ModalViewModel = new ModalViewModel();

    /* tooltips for icons */
    $(function () {
        $('.icon-file').tooltip();
        $('.icon-folder-open').tooltip();
        $('.icon-download-alt').tooltip();
    });

    /* disable selection on certain items to enhance UX */
    // aka we don't want shit highlighting everywhere when I click to drag/select etc
    $("ul, li, i").disableSelection();
        

    /*
    function itemDragHelper() {
        var z = $(event.target).closest('li').find('.name-col');
        return z.hasClass("folder") ? $('<div class="drag-table-item"><i class="icon-folder-close"></i>' + z.text() + '</div>') : 
            $('<div class="drag-table-item"><i class="icon-file"></i>' + z.text() + '</div>');
    };*/

    /* DROPPABLE 
    droppableItems.droppable({
        accept: draggableItems,
        activeClass: onPickUp,
        drop: onDrop,
        over: onFolderHover,
        out: onFolderHoverExit,
    });
    */

    ko.applyBindings(viewModel);
});