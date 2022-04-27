


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
        ids : [{"id" : "123123", "command" : ""}],
        suggested : ["SPECTATE|nohumanman", "SET_BIKE|1", "FREEZE_PLAYER", "UNFREEZE_PLAYER", "TOGGLE_CONTROL|true", "CLEAR_SESSION_MARKER", "RESET_PLAYER", "ADD_MODIFIER|FAKIEBALANCE", "ADD_MODIFIER|PUMPSTRENGTH", "RESPAWN_ON_TRACK", "RESPAWN_AT_START"],
    },
    methods: {
        SubmitEval(id){
            $.get("/eval/" + id.id + "?order=" + id.command);
        }
    }
});

$.getJSON("/get", function(data){app.ids = data["ids"]})