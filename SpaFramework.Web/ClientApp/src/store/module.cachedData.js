import Vue from 'vue';
import Vuex from 'vuex';
import axios from "axios";

const STATE_UNLOADED = 0;
const STATE_LOADING = 1;
const STATE_LOADED = 2;

export default {
  namespaced: true,
  state: () => ({
    stateSelectOptions: [
      { text: "", value: null },
      { text: "Alabama", value: "AL" },
      { text: "Alaska", value: "AK" },
      { text: "American Samoa", value: "AS" },
      { text: "Arizona", value: "AZ" },
      { text: "Arkansas", value: "AR" },
      { text: "California", value: "CA" },
      { text: "Colorado", value: "CO" },
      { text: "Connecticut", value: "CT" },
      { text: "Delaware", value: "DE" },
      { text: "District Of Columbia", value: "DC" },
      { text: "Federated States Of Micronesia", value: "FM" },
      { text: "Florida", value: "FL" },
      { text: "Georgia", value: "GA" },
      { text: "Guam", value: "GU" },
      { text: "Hawaii", value: "HI" },
      { text: "Idaho", value: "ID" },
      { text: "Illinois", value: "IL" },
      { text: "Indiana", value: "IN" },
      { text: "Iowa", value: "IA" },
      { text: "Kansas", value: "KS" },
      { text: "Kentucky", value: "KY" },
      { text: "Louisiana", value: "LA" },
      { text: "Maine", value: "ME" },
      { text: "Marshall Islands", value: "MH" },
      { text: "Maryland", value: "MD" },
      { text: "Massachusetts", value: "MA" },
      { text: "Michigan", value: "MI" },
      { text: "Minnesota", value: "MN" },
      { text: "Mississippi", value: "MS" },
      { text: "Missouri", value: "MO" },
      { text: "Montana", value: "MT" },
      { text: "Nebraska", value: "NE" },
      { text: "Nevada", value: "NV" },
      { text: "New Hampshire", value: "NH" },
      { text: "New Jersey", value: "NJ" },
      { text: "New Mexico", value: "NM" },
      { text: "New York", value: "NY" },
      { text: "North Carolina", value: "NC" },
      { text: "North Dakota", value: "ND" },
      { text: "Northern Mariana Islands", value: "MP" },
      { text: "Ohio", value: "OH" },
      { text: "Oklahoma", value: "OK" },
      { text: "Oregon", value: "OR" },
      { text: "Palau", value: "PW" },
      { text: "Pennsylvania", value: "PA" },
      { text: "Puerto Rico", value: "PR" },
      { text: "Rhode Island", value: "RI" },
      { text: "South Carolina", value: "SC" },
      { text: "South Dakota", value: "SD" },
      { text: "Tennessee", value: "TN" },
      { text: "Texas", value: "TX" },
      { text: "Utah", value: "UT" },
      { text: "Vermont", value: "VT" },
      { text: "Virgin Islands", value: "VI" },
      { text: "Virginia", value: "VA" },
      { text: "Washington", value: "WA" },
      { text: "West Virginia", value: "WV" },
      { text: "Wisconsin", value: "WI" },
      { text: "Wyoming", value: "WY" }
    ],
    projectStateSelectOptions: [
      {
        text: '',
        value: null
      },
      {
        text: 'Open',
        value: 'Open'
      },
      {
        text: 'On Hold',
        value: 'OnHold'
      },
      {
        text: 'Closed',
        value: 'Closed'
      }
    ],
    clients: {
      loadState: STATE_UNLOADED,
      values: [],
      selectOptions: []
    }
  }),
  mutations: {
    SET_CLIENTS_LOAD_STATE(state, loadState) {
      state.clients.loadState = loadState;
    },
    LOAD_CLIENTS(state, values) {
      state.clients.values = values;

      state.clients.selectOptions = values.map(x => ({
        text: x.abbreviation + ' (' + x.name + ')',
        value: x.id
      }));

      state.clients.selectOptions.unshift({
        text: '',
        value: null
      });

      state.clients.loadState = STATE_LOADED;
    },
  },
  actions: {
    loadValues({ state, commit, rootState }, valType) {
      return new Promise((resolve, reject) => {
        if (state[valType.type].loadState == STATE_LOADED)
        {
          resolve();
          return;
        }

        if (state[valType.type].loadState == STATE_LOADING)
          return;

        commit('SET_' + valType.commitType + '_LOAD_STATE', STATE_LOADING);

        let url = valType.url;
        url += '?limit=1000';

        if (valType.includes && valType.includes.length > 0)
          url += '&includes=' + valType.includes.join(',')

        if (valType.order)
          url += '&order=' + encodeURIComponent(valType.order);

        axios
          .get(url)
          .then(response => {
            commit('LOAD_' + valType.commitType, response.data);
            resolve();
          })
          .catch(error => {
            console.log(error);

            reject();
          });
      });
    },
    loadClients({ commit, dispatch }) {
      return dispatch('loadValues', { 
        type: 'clients', 
        commitType: 'CLIENTS', 
        url: '/api/client',
        order: 'name'
      });
    }
  },
  getters: {
  }
}