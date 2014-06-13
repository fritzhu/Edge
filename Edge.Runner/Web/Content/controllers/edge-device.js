(function () {
    var app = angular.module('edge');

    app.directive('edgeDevice', function () {
        return {
            restrict: 'E',
            templateUrl: 'partials/edge-device.tpl.html',
            scope: {
                deviceId: '=',
                deviceName: '=',
                memberName: '='
            },
            controllerAs: 'device',
            controller: function ($scope, $http, $interval) {
                $scope.state = null;

                $http({ method: 'GET', url: '/device/' + $scope.deviceId + '/' + $scope.memberName })
                    .success(function (data) {
                        $scope.state = data;
                    });

                $scope.stopUpdate = $interval(function () {
                    $http({ method: 'GET', url: '/device/' + $scope.deviceId + '/' + $scope.memberName })
                        .success(function (data) {
                            $scope.state = data;
                        });
                }, 1000);

                $scope.setState = function (to) {
                    $http({ method: 'GET', url: '/device/' + $scope.deviceId + '/' + $scope.memberName + '/' + to });
                    $scope.state = to;
                };

                $scope.$on('$destroy', function () {
                    if (angular.isDefined($scope.stopUpdate)) {
                        $interval.cancel($scope.stopUpdate);
                        $scope.stopUpdate = undefined;
                    }
                });
            }
        };
    });
})();