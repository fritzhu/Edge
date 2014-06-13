(function () {
    var app = angular.module('edge', ['ngRoute', 'ui.bootstrap']);

    app.config(['$routeProvider',
        function ($routeProvider) {
            $routeProvider
                .when('/rooms/:roomId', {
                    templateUrl: 'partials/room.view.html',
                    controller: 'RoomViewController'
                })
                .otherwise({
                    redirectTo: '/rooms/House'
                });
        }
    ]);

    app.controller('NavbarController', function ($route, $scope, $http) {
        $scope.rooms = [];
        $http({ method: 'GET', url: '/zones/list' })
            .success(function (data) {
                $scope.rooms = data;
            })
            .error(function (data) {
                alert(data);
            });
    });

    app.directive('navbarItem', function () {
        return {
            restrict: 'E',
            replace: true,
            templateUrl: 'partials/navbar-item.tpl.html',
            scope: {
                path: '=',
                title: '='
            },
            controllerAs: 'navbar',
            controller: function ($location, $scope) {
                this.isCurrentTab = function (r) {
                    return $location.path() === $scope.path
                };
            }
        };
    });

})();