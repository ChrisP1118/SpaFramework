<template>
  <list-page-template page-title="Clients">
    <filtered-table :settings="tableSettings" @rowClicked="onRowClicked" @newClicked="onNewClicked">
    </filtered-table>
  </list-page-template>
</template>

<script>
import axios from "axios";

export default {
  name: "ClientList",
  props: {},
  data() {
    let base = this;
    return {
      tableSettings: {
        endpoint: '/api/client',
        showNewButton: true,
        columns: [
          {
            key: 'id',
            name: 'ID',
            visible: false,
            sortable: true,
            type: 'number'
          },
          {
            key: 'name',
            name: 'Name',
            visible: true,
            sortable: true,
            type: 'text'
          },
          {
            key: 'abbreviation',
            name: 'Abbreviation',
            visible: true,
            sortable: true,
            type: 'text'
          },
          {
            key: 'clientStats.numberOfProjects',
            name: 'Number of Projects',
            visible: true,
            sortable: true,
            type: 'number'
          },
          {
            key: 'clientStats.firstStartDate',
            name: 'First Start Date',
            visible: false,
            sortable: true,
            type: 'date'
          },
          {
            key: 'clientStats.lastEndDate',
            name: 'Last End Date',
            visible: true,
            sortable: true,
            type: 'date'
          },
          {
            key: 'lastModification',
            name: 'Last Modification',
            visible: false,
            sortable: true,
            type: 'date'
          }
        ],
        getDefaultFilter: function () {
          return '';
        },
        includes: [
          'clientStats'
        ]
      }
    }
  },
  methods: {
    onRowClicked: function (item, context) {
      this.$router.push('/client/' + item.id);
    },
    onNewClicked: function (filters) {
      this.$router.push('/client/add');
    }
  },
  computed: {
  },
  mounted () {
  }
};
</script>