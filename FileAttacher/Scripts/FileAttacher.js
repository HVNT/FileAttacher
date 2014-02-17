
var viewModel = {};

$(document).ready(function () {

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

    ko.applyBindings(viewModel);
});