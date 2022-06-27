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
        self_to_submit: "",
        spectating: "",
    },
    methods: {
        setSelf(steam_id){
            this.self = steam_id
            $.get("/submit-steam-id", data={"steam_id": app.self})
            // window.localStorage.setItem('self', steam_id);
        },
    }
});

app.self = window.localStorage.getItem('self');

function updateSpectatedPlayer(){
    if (app.self != null){
        $.get("/get-spectating", data={"steam_id": app.self}, function(data){
            app.spectating = data;
        });
    }
}
updateSpectatedPlayer();
setInterval(updateSpectatedPlayer, 500);
