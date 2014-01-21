var viewModel = {};
viewModel.MainViewModel = new MainViewModel();
viewModel.ModalViewModel = new ModalViewModel();

$(document).ready(function () {
    
    ko.applyBindings(viewModel);
});