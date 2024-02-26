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
        spectating: "",
    },
    methods: {
        updateSpectatedPlayer(){
            $.get("/api/get-spectated", data={"my_id": this.getSelf()}, function(data){
                if (data.startsWith("Failed"))
                    app.spectating = ""
                else
                    app.spectating = data;
            });
        },
        getSelf(){
            const params = new URLSearchParams(window.location.search)
            return params.get('id')
        }
    }
});

app.updateSpectatedPlayer();
setInterval(app.updateSpectatedPlayer, 1000);