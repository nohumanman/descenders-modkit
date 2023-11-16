var app = new Vue({
    el: '#app',
    delimiters: ['[[', ']]'],
    vuetify : new Vuetify({
        theme: {
            dark : {
                primary: '#1976D2',
                secondary: '#424242',
                accent: '#82B1FF',
                error: '#FF5252',
                info: '#2196F3',
                success: '#4CAF50',
                warning: '#FFC107',
            }
    },
    }),
    data : {
        self: null,
        spectating: "",
        players: []
    },
    methods: {
        setSelf(steam_id){
            this.self = steam_id
        },
        updatePlayers(){
            $.get("/get", function(data){
                app.players = data["ids"];
            });
        },
        updateSpectatedPlayer(){
            if (app.self != null){
                $.get("/get-spectated", data={"steam_id": app.self}, function(data){
                    app.spectating = data;
                });
            }
        }
    }
});

setInterval(app.updatePlayers, 500);

app.updateSpectatedPlayer();
setInterval(app.updateSpectatedPlayer, 500);
