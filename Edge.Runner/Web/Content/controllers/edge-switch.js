(function () {
    var app = angular.module('edge');

    app.directive('edgeSwitch', function () {
        return {
            restrict: 'E',
            templateUrl: 'partials/edge-switch.tpl.html',
            scope: {
                offLabel: '=',
                onLabel: '=',
                state: '=',
                stateController: '='
            },
            controllerAs: 'switch',
            controller: function ($scope) {
                $scope.setState = function (val) {
                    $scope.stateController.setState(val);
                };
            }
        };
    });
})();