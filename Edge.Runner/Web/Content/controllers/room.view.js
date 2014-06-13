(function() {
    var app = angular.module('edge');

    app.controller('RoomViewController', function ($scope, $routeParams, $http, $interval) {
        var roomId = $routeParams.roomId;

        $scope.devices = [];
        $scope.activeScene = null;
        
        $http({ method: 'GET', url: '/zones/' + roomId })
            .success(function (data) {
                $scope.scenes = data.scenes;
                $scope.activeScene = data.activeScene;
            });

        $scope.activateScene = function (scene) {
            $http({ method: 'GET', url: '/zones/' + roomId + '/scene/' + scene.id })
            .success(function () {
                $scope.activeScene = scene;
            });
        };

        $scope.stopUpdate = $interval(function () {
            $http({ method: 'GET', url: '/zones/' + roomId })
                .success(function (data) {
                    if (!$scope.activeScene || !data.activeScene || $scope.activeScene.id !== data.activeScene.id) {
                        $scope.activeScene = data.activeScene
                    }
                });
        }, 1000);

        $scope.$on('$destroy', function () {
            if (angular.isDefined($scope.stopUpdate)) {
                $interval.cancel($scope.stopUpdate);
                $scope.stopUpdate = undefined;
            }
        });
    });

})();
