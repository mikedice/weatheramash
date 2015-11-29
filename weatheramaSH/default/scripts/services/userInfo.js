app.service('userInfo', ['$rootScope', function ($rootScope) {
    return {
        user: {},
        setUser: function (userInfo) {
            user = userInfo
            $rootScope.$broadcast('UserUpdated', user);
        },
        getUser: function()
        {
            return user;
        }
    }

}]);