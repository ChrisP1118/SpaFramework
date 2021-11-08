<template>
  <div>
    <b-row v-if="!settings.hideToolbar">
      <b-col>
        <span class="float-right pt-2">
          {{resultsTotal}}
        </span>
        <b-button-toolbar class="mb-2">
          <b-button-group v-if="showNewLink">
            <b-button :to="settings.newUrl"><b-icon icon="plus-circle" /> New</b-button>
          </b-button-group>
          <b-button-group v-if="showNewFunction">
            <b-button :to="settings.newUrl()"><b-icon icon="plus-circle" /> New</b-button>
          </b-button-group>
          <b-button-group v-if="settings.showNewButton">
            <b-button @click="newClicked"><b-icon icon="plus-circle" /> New</b-button>
          </b-button-group>
          <b-button-group :class="(settings.newUrl || settings.showNewButton) ? 'ml-2' : ''">
            <b-dropdown>
              <template slot="button-content">
                <b-icon icon="table" />
              </template>
              <b-dropdown-item-button v-for="column in settings.columns" v-bind:key="column.key" v-on:click="toggleColumn(column)">
                <b-icon icon="square" v-if="!column.visible" />
                <b-icon icon="check-square" v-if="column.visible" />
                {{column.name}}
                </b-dropdown-item-button>
            </b-dropdown>
            <b-dropdown :text="limit.toString()">
              <b-dropdown-item-button v-for="limit in limits" v-bind:key="limit" v-on:click="setLimit(limit)">{{limit}}</b-dropdown-item-button>
            </b-dropdown>
          </b-button-group>
          <b-button-group class="ml-2" v-if="settings.globalSearch && !isSearchVisible">
            <b-button @click="isSearchVisible = true"><b-icon icon="search" /></b-button>
          </b-button-group>
          <b-input-group class="ml-2" v-if="settings.globalSearch && isSearchVisible">
            <template #prepend>
              <b-button @click="isSearchVisible = false"><b-icon icon="search" /></b-button>
            </template>
            <b-form-input size="lg" placeholder="Search" v-model="searchTerm" @keyup="refreshData(true, true)"></b-form-input>
            <template #append>
              <b-button :disabled="!anyFiltersOrSearch" @click="clearFiltersAndSearch"><b-icon icon="x-circle"></b-icon></b-button>
            </template>
          </b-input-group>
          <b-button-group class="ml-2">
            <b-dropdown>
              <template slot="button-content">
                <b-icon icon="file-earmark-spreadsheet" /> Export
              </template>
              <b-dropdown-group header="Export Options">
                <b-dropdown-item-button @click="exportType = 'Page'"><b-icon :icon="exportType == 'Page' ? 'check-circle' : 'circle'" /> This Page</b-dropdown-item-button>
                <b-dropdown-item-button @click="exportType = 'All'"><b-icon :icon="exportType == 'All' ? 'check-circle' : 'circle'" /> All Results</b-dropdown-item-button>
              </b-dropdown-group>
              <b-dropdown-divider></b-dropdown-divider>
              <b-dropdown-group header="Export Results">
                <b-dropdown-item-button @click="exportResults('CSV')">Download CSV</b-dropdown-item-button>
                <b-dropdown-item-button @click="exportResults('PDF')">Download PDF</b-dropdown-item-button>
                <b-dropdown-item-button @click="exportResults('ViewPDF')">View PDF</b-dropdown-item-button>
              </b-dropdown-group>
            </b-dropdown>
          </b-button-group>
          <slot name="extraButtons"></slot>
        </b-button-toolbar>
      </b-col>
    </b-row>
    <b-row>
      <b-col>
        <b-table-simple striped hover responsive="true">
          <b-thead>
            <b-tr>
              <b-th v-for="column in visibleColumns" v-bind:key="column.key" v-on:click="setSortColumn(column)">
                {{column.name}}
                <span v-if="column.key == sortColumn">
                  <b-icon :icon="sortOrder == 'asc' ? 'sort-down' : 'sort-up'" />
                </span>
              </b-th>
            </b-tr>
            <b-tr>
              <b-th v-for="column in visibleColumns" v-bind:key="column.key">
                <b-form-input v-model="filters[column.key]" v-if="!column.hideFilter && getFilterType(column) == 'text'" @keyup="onFilterKeyUp"></b-form-input>
                <b-form-input v-model="filters[column.key]" v-if="!column.hideFilter && getFilterType(column) == 'number'" type="number" @keyup="onFilterKeyUp"></b-form-input>
                <b-form-input v-model="filters[column.key]" v-if="!column.hideFilter && getFilterType(column) == 'date'" type="date" @change="refreshData(true, true)"></b-form-input>
                <b-form-select v-model="filters[column.key]" v-if="!column.hideFilter && getFilterType(column) == 'select'" :options="column.selectOptions" @change="refreshData(false, true)"></b-form-select>
                <b-row v-if="!column.hideFilter && getFilterType(column) == 'daterange'">
                  <b-col xs="12" md="6">
                    <b-form-input v-if="filters[column.key]" v-model="filters[column.key].low" type="date" @change="refreshData(false, true)"></b-form-input>
                  </b-col>
                  <b-col xs="12" md="6">
                    <b-form-input v-if="filters[column.key]" v-model="filters[column.key].high" type="date" @change="refreshData(false, true)"></b-form-input>
                  </b-col>
                </b-row>

                <!-- https://bootstrap-vue.org/docs/components/form-tags#using-custom-form-components -->
                <!--
                <b-form-group v-if="!column.hideFilter && getFilterType(column) == 'multiselect'">
                  <b-form-tags id="tags-with-dropdown" v-model="filters[column.key]" no-outer-focus class="mb-2">
                    <template v-slot="{ tags, disabled, addTag, removeTag }">
                      <ul v-if="tags.length > 0" class="list-inline d-inline-block mb-2">
                        <li v-for="tag in tags" :key="tag" class="list-inline-item">
                          <b-form-tag @remove="removeTag(tag)" :title="tag" :disabled="disabled" variant="info">
                            {{column.selectOptions.find(c => c.value == tag).text}}
                          </b-form-tag>
                        </li>
                      </ul>
                      <b-dropdown size="sm" variant="outline-secondary" block menu-class="w-100">
                        <template #button-content>
                          <b-icon icon="tag-fill"></b-icon> Choose tags
                        </template>
                        <b-dropdown-item-button v-for="option in column.selectOptions" :key="option.value" @click="onMultiselectOptionClick({ option, addTag })">
                          {{ option }}
                        </b-dropdown-item-button>
                      </b-dropdown>
                    </template>
                  </b-form-tags>
                </b-form-group>
                -->
                <b-form-group v-if="!column.hideFilter && getFilterType(column) == 'multiselect'">
                  <b-form-tags v-model="filters[column.key]" add-on-change no-outer-focus @input="multiSelectUpdated">
                    <template v-slot="{ tags, inputAttrs, inputHandlers, disabled, addTag, removeTag }">
                      <ul v-if="tags.length > 0" class="list-inline d-inline-block mb-2">
                        <li v-for="tag in tags" :key="tag" class="list-inline-item">
                          <b-form-tag @remove="removeTag(tag)" :title="tag" :disabled="disabled" variant="secondary">
                            {{column.selectOptions.find(c => c.value == tag).text}}
                          </b-form-tag>
                        </li>
                      </ul>
                      <!-- According to the docs, you shouldn't need this change event. And it's a major hack. But it seems to work -- without it, tags are never added. -->
                      <b-form-select v-bind="inputAttrs" v-on="inputHandlers" :options="column.selectOptions" @change="addMultiselectTag(addTag, inputAttrs);">
                        <template #first>
                          <option disabled value=""></option>
                        </template>
                      </b-form-select>
                    </template>
                  </b-form-tags>
                </b-form-group>
              </b-th>
            </b-tr>
            <b-tr v-if="refreshingData">
              <b-td :colspan="visibleColumns.length">
                <div class="text-center text-info my-2">
                  <b-spinner class="align-middle"></b-spinner>
                  <strong>Loading...</strong>
                </div>
              </b-td>
            </b-tr>
            <b-tr v-if="dataError">
              <b-td :colspan="visibleColumns.length">
                <div class="text-center text-danger my-2">
                  {{ dataError }}
                </div>
              </b-td>
            </b-tr>
          </b-thead>
          <b-tbody v-if="!refreshingData">
            <b-tr v-for="(item, index) in items" v-bind:key="index" @click="rowClicked(item, index)">
              <b-td v-for="column in visibleColumns" v-bind:key="column.key" v-html="highlightSearchTerms(item[column.key], column.key)">
              </b-td>
            </b-tr>
          </b-tbody>
        </b-table-simple>
        <span>
          {{resultsTotal}}
        </span>
        <b-pagination v-model="page" :total-rows="total" :per-page="limit" @input="refreshData"></b-pagination>
      </b-col>
    </b-row>
    <b-modal id="modalExportResults" hide-footer header-bg-variant="secondary" header-text-variant="light">
      <template #modal-title>
        Export Results
      </template>
      <div>
        <b-spinner label="Spinning"></b-spinner>
      </div>
      <p>Please wait, downloading results...</p>
      <p>{{exportStep}}</p>
    </b-modal>
    <b-modal id="modalViewPDF" size="xl" hide-footer @shown="onModalViewPDFShown" header-bg-variant="secondary" header-text-variant="light">
      <template #modal-title>
        View PDF
      </template>
      <object ref="viewPDFResults" style="height: 80vh; width: 100%">
      </object>
    </b-modal>
  </div>
</template>

<script>
import axios from "axios";
import { DateTime } from 'luxon';
import { saveAs } from 'file-saver';
import { mapState, mapGetters } from 'vuex'
import jsPDF from 'jspdf'
import 'jspdf-autotable'

const columnTypes = {
  text: {
    filterType: 'text',
    render: (value, row, col) => {
      return value;
    },
    apply: (value, col) => {
      if (value == '')
        return null;

      if (col.filterMethod == 'contains')
        return col.key + '.Contains("' + value + '")';
      else
        return col.key + '.StartsWith("' + value + '")';
    },
    highlight: (terms, value, col) => {
      let v = value;
      terms.forEach(term => {
        let esc = term.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&')
        let re = new RegExp(esc, "gi");
        v = v.replace(re, '<span class="bg-info">$&</span>');
      });

      return v;
    },
    search: (term, col) => {
      return col.key + '.Contains("' + term + '")';
    }
  },
  phone: {
    filterType: 'text',
    render: (value, row, col) => {
      if (!value)
        return value;

      if (value.length != 10)
        return value;

      return value.substring(0, 3) + '-' + value.substring(3, 6) + '-' + value.substring(6, 10);
    },
    apply: (value, col) => {
      if (value == '')
        return null;

      // Remove hyphens, spaces, and parentheses
      let v = value.replace(/[- ()]/g, "");

      return col.key + '.StartsWith("' + v + '")';
    },
    highlight: (terms, value, col) => {
      let v = value;
      terms.forEach(term => {
        let esc = term.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&')
        let re = new RegExp(esc, "gi");
        v = v.replace(re, '<span class="bg-info">$&</span>');
      });

      return v;
    },
    search: (term, col) => {
      return col.key + '.Contains("' + term + '")';
    }
  },
  guid: {
    filterType: 'text',
    render: (value, row, col) => {
      return value;
    },
    apply: (value, col) => {
      if (!value || value.length == 0)
        return null;

      return col.key + '="' + value + '"';
    },
    highlight: (terms, value, col) => {
      let v = value;
      terms.forEach(term => {
        let esc = term.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&')
        let re = new RegExp(esc, "gi");
        v = v.replace(re, '<span class="bg-info">$&</span>');
      });

      return v;
    },
    search: (term, col) => {
      return col.key + '="' + term + '"';
    }
  },
  number: {
    filterType: 'number',
    render: (value, row, col) => {
      return value;
    },
    apply: (value, col) => {
      if (value == '')
        return '';
      return col.key + '=' + value;
    }
  },
  date: {
    filterType: 'date',
    render: (value, row, col) => {
      if (!value)
        return '';
        
      let dt = DateTime.fromISO(value);
      return dt.toLocaleString(DateTime.DATE_MED_WITH_WEEKDAY);
    },
    apply: (value, col) => {
      let d = DateTime.fromISO(value).plus({ days: 1});
      let e = DateTime.fromISO(value);
      return col.key + '>="' + e.toFormat('yyyy-MM-dd') + '" and ' + col.key + '<"' + d.toFormat('yyyy-MM-dd') + '"';
    }
  },
  datetime: {
    filterType: 'date',
    render: (value, row, col) => {
      if (!value)
        return '';

      let dt = DateTime.fromISO(value);
      return dt.toLocaleString(DateTime.DATE_MED_WITH_WEEKDAY) + ' ' + dt.toLocaleString(DateTime.TIME_SIMPLE);
    },
    apply: (value, col) => {
      let d = DateTime.fromISO(value).plus({ days: 1});
      let e = DateTime.fromISO(value);
      return col.key + '>="' + e.toFormat('yyyy-MM-dd') + '" and ' + col.key + '<"' + d.toFormat('yyyy-MM-dd') + '"';
    }
  },
  datetimeoffset: {
    filterType: 'date',
    render: (value, row, col) => {
      if (!value)
        return '';
        
      let dt = DateTime.fromISO(value);
      return dt.toLocaleString(DateTime.DATE_MED_WITH_WEEKDAY) + ' ' + dt.toLocaleString(DateTime.TIME_WITH_SHORT_OFFSET);
    },
    apply: (value, col) => {
      let d = DateTime.fromISO(value).plus({ days: 1});
      let e = DateTime.fromISO(value);
      return col.key + '>="' + e.toFormat('yyyy-MM-dd') + '" and ' + col.key + '<"' + d.toFormat('yyyy-MM-dd') + '"';
    }
  },
  daterange: {
    filterType: 'daterange',
    init: () => {
      return {
        low: null,
        high: null
      };
    },
    render: (value, row, col) => {
      if (!value)
        return '';
        
      let dt = DateTime.fromISO(value);
      return dt.toLocaleString(DateTime.DATE_MED_WITH_WEEKDAY);
    },
    apply: (value, col) => {
      if (value.low && value.high) {
        let l = DateTime.fromISO(value.low);
        let h = DateTime.fromISO(value.high).plus({ days: 1});
        return col.key + '>="' + l.toFormat('yyyy-MM-dd') + '" and ' + col.key + '<"' + h.toFormat('yyyy-MM-dd') + '"';
      } else if (value.low) {
        let l = DateTime.fromISO(value.low);
        return col.key + '>="' + l.toFormat('yyyy-MM-dd') + '"';
      } else if (value.high) {
        let h = DateTime.fromISO(value.high).plus({ days: 1});
        return col.key + '<"' + h.toFormat('yyyy-MM-dd') + '"';
      } else return '';
    }
  },
  select: {
    filterType: 'select',
    render: (value, row, col) => {
      let selectItem = col.selectOptions ? col.selectOptions.find(c => c.value == value) : null;
      if (selectItem)
        return selectItem.text;
      else
        return null;
    },
    apply: (value, col) => {
      return col.key + '="' + value + '"';
    }
  },
  multiselect: {
    filterType: 'multiselect',
    render: (value, row, col) => {
      if (!col.displaySelector)
        return null;

      let r = [];
      value.forEach(z => {
        let q = col.displaySelector.split('.');
        let y = z;
        q.forEach(k => {
          y = y[k];
        })
        r.push(y);
      });
      return r.join(', ');
    },
    apply: (value, col) => {
      if (!Array.isArray(value) || value.length == 0)
        return null;

      let b = value.map(x => col.key + '.Any(x => x.' + col.filterSelector + '="' + x + '")');
      return '(' + b.join(' OR ') + ')';
    }
  }
}

export default {
  name: "FilteredTable",
  props: {
    settings: {
      type: Object
    },
    defaultFilters: {},
  },
  data() {
    return {
      limit: 10,
      limits: [10, 25, 50, 100, 500, 1000],
      filters: {},
      rawItems: [],
      items: [],
      total: 0,
      totalMax: -1,
      page: 1,
      sortColumn: null,
      sortOrder: 'asc',
      refreshDataTimeout: null,
      refreshingData: false,
      refreshDataTime: null,
      isSearchVisible: false,
      searchTerm: null,
      dataError: null,
      exportType: 'Page',
      exportStep: '',
      viewPDFResults: null
    }
  },
  computed: {
    anyFiltersOrSearch: function () {
      if (this.searchTerm && this.searchTerm.length > 0)
        return true;

      for (var propertyName in this.filters)
        if (this.filters[propertyName] || this.filters[propertyName] === false)
          return true;

      return false;
    },
    visibleColumns: function () {
      return this.settings.columns.filter(c => c.visible);
    },
    visibleFields: function () {
      return this.settings.columns.filter(c => c.visible && !c.key.startsWith('_')).map(c => c.key);
    },
    resultsTotal: function () {
      let startIndex = this.page * this.limit - this.limit + 1;
      let endIndex = this.page * this.limit > this.total ? this.total : this.page * this.limit;
      let total = this.total;

      if (total == 0)
        return 'No results';
      else
        return startIndex + '-' + endIndex + ' of ' + (total == this.totalMax ? 'over ' : '') + total + ' results';
    },
    showNewLink: function () {
      return this.settings.newUrl && typeof(this.settings.newUrl) !== 'function';
    },
    showNewFunction: function () {
      return this.settings.newUrl && typeof(this.settings.newUrl) === 'function';
    }
  },
  methods: {
    getColumnType: function (col) {
      let colType = col.type;
      if (!colType)
        colType = 'text';

      return columnTypes[colType];
    },
    getFilterType: function (col) {
      if (!col)
        return null;

      if (col.filterType)
        return col.filterType;

      return this.getColumnType(col).filterType;
    },
    // onMultiselectOptionClick: function ({option, addTag}) {
    //   addTag(option.value);
    // },
    addMultiselectTag: function (addTag, inputAttrs) {
      addTag(window.document.getElementById(inputAttrs.id).value);
    },
    exportResults: function (fileType) {
      let url = this.getDataUrl(this.exportType == 'All');

      this.exportStep = 'Downloading...';
      this.$bvModal.show('modalExportResults');

      axios
        .get(url)
        .then(response => {
          this.exportStep = 'Processing...';
          let exportItems = response.data;

          if (fileType == 'CSV') {
            let rows = [];
            let cols = [];

            this.settings.columns.forEach(col => {
              if (col.visible)
                cols.push(col.name);
            });
            rows.push(cols.join(','));

            exportItems.forEach(row => {
              cols = [];
              this.settings.columns.forEach(col => {
                if (col.visible)
                  cols.push('"' + this.getCellValue(row, col) + '"');
              });

              rows.push(cols.join(','));
            });

            let output = rows.join('\n');

            this.exportStep = 'Exporting...';

            let blob = new Blob([output], {type: "text/csv;charset=utf-8"});
            var FileSaver = require('file-saver');
            FileSaver.saveAs(blob, "export.csv");
          } else if (fileType == 'PDF' || fileType == 'ViewPDF') {
            let head = [];
            let body = [];

            this.settings.columns.forEach(col => {
              if (col.visible)
                head.push(col.name);
            });

            exportItems.forEach(row => {
              let cols = [];
              this.settings.columns.forEach(col => {
                if (col.visible)
                  cols.push(this.getCellValue(row, col));
              });

              body.push(cols);
            });

            let doc = new jsPDF('l', 'in', [8.5, 11], true);

            doc.autoTable({
              head: [head],
              body: body,
              styles: {
                font: 'Helvetica',
                fontSize: 9
              }
            });

            if (fileType == 'PDF') {
              doc.save('results.pdf');
            } else if (fileType == 'ViewPDF') {
              this.viewPDFResults = doc.output('datauristring');

              this.$bvModal.hide('modalExportResults');
              this.$bvModal.show('modalViewPDF');
            }
          }

          this.$bvModal.hide('modalExportResults');
        })
        .catch(err => {
          console.log(err);
          this.$bvModal.hide('modalExportResults');
        });
    },
    onModalViewPDFShown: function () {
      this.$refs.viewPDFResults.data = this.viewPDFResults;
    },
    multiSelectUpdated: function () {
      this.refreshData(false, true);
    },
    clearFiltersAndSearch: function () {
      this.searchTerm = null;

      for (var propertyName in this.filters)
        this.filters[propertyName] = null;

      this.refreshData(false, true);
    },
    highlightSearchTerms: function (val, colKey) {
      if (!val)
        return val;

      let searchTerms = this.getSearchTerms();
      let filter = typeof this.filters[colKey] == 'string' ? this.splitSearchTerms(this.filters[colKey]) : [];
      let terms = [...searchTerms, ...filter];

      if (terms && terms.length > 0) {
        let col = this.settings.columns.find(c => c.key == colKey);

        let fn = this.getColumnType(col).highlight;
        if (fn)
          val = fn(terms, val, col);
      }

      return val;
    },
    getSearchTerms: function () {
      return this.splitSearchTerms(this.searchTerm);
    },
    splitSearchTerms: function (searchTerms) {
      if (!searchTerms)
        return [];

      let terms = searchTerms.split(/[\s,]+/);
      let outTerms = [];
      terms.forEach(term => {
        let trimTerm = term.trim();
        if (trimTerm.length > 0)
          outTerms.push(trimTerm);
      });

      return outTerms;
    },
    newClicked: function () {
      this.$emit('newClicked', this.filters);
    },
    rowClicked: function (item, index) {
      let rawItem = this.rawItems[index];
      this.$emit('rowClicked', rawItem);
    },
    toggleColumn: function (column) {
      column.visible = !column.visible;
      this.refreshItems();
    },
    setLimit: function (newLimit) {
      this.limit = newLimit;
      this.refreshData();
    },
    setSortColumn: function (column) {
      if (!column.sortable)
        return;

      if (this.sortColumn == column.key)
        this.sortOrder = this.sortOrder == 'asc' ? 'desc' : 'asc';
      else {
        this.sortColumn = column.key;
        this.sortOrder = 'asc';
      }

      this.refreshData();
    },
    onFilterKeyUp: function (e) {
      if (e.keyCode >= 9 && e.keyCode < 46 && e.keyCode != 13)
        return;

      this.refreshData(true, true);
    },
    refreshItems: function () {
      // Recreates items from rawItems - doesn't actually go back to server for different data
      this.items = [];
      this.rawItems.forEach(row => {
        let item = {};
        this.settings.columns.forEach(col => {
          item[col.key] = this.getCellValue(row, col);
          item['_raw'] = row;
          row['_item'] = item;
        });

        this.items.push(item);
      });
    },
    getCellValue: function (row, col) {
      if (col.visible) {
        let p = col.key.split('.');
        let v = row;
        p.forEach(k => {
          if (v === undefined || v === null)
            v = null;
          else
            v = v[k];
        });

        if (col.render)
          return col.render(v, row);
        else
          return this.getColumnType(col).render(v, row, col);
      }
    },
    getFilterUrlComponent: function () {
      let filters = [];
      if (this.settings.getDefaultFilter)
        filters.push(this.settings.getDefaultFilter());

      this.settings.columns.forEach(col => {
        let f = this.filters[col.key];
        if (f === undefined || f === null)
          return;

        let c = this.getColumnType(col);
        let r = null;

        if (col.filterFormatter)
          f = col.filterFormatter(f, col);

        if (col.customFilter)
          r = col.customFilter(f, col);
        else
          r = c.apply(f, col);

        if (r !== null)
          filters.push(r);
      });

      if (this.searchTerm) {
        let terms = this.getSearchTerms();
        terms.forEach(term => {
          let t = [];
          this.visibleColumns.forEach(col => {

            let fn = this.getColumnType(col).search;
            if (fn) {
              let val = fn(term, col);

              if (val !== null)
                t.push(val);
            }
          });

          filters.push('(' + t.join(' OR ') + ')');
        });
      }

      let filter = filters.filter(f => f.length > 0).join(' AND ');
      return filter;
    },
    getDataUrl: function (returnAll) {
      // Refreshes rawItems -- hits the server to get new data
      let url = this.settings.endpoint + '?';
      let offset = (this.page * this.limit) - this.limit;

      if (returnAll) {
        url += "&limit=10000";
      } else {
        url += '&offset=' + offset;
        url += '&limit=' + this.limit;
      }

      if (this.settings.maxCount)
        url += '&maxCount=' + this.settings.maxCount;

      if (this.sortColumn)
        url += '&order=' + encodeURIComponent(this.sortColumn + ' ' + this.sortOrder);

      if (this.settings.includes)
        url += '&includes=' + encodeURIComponent(this.settings.includes.join(','));

      let filter = this.getFilterUrlComponent();

      if (filter.length > 0)
        url += '&filter=' + encodeURIComponent(filter);

      return url;
    },
    refreshData: function (wait, resetPaging) {
      if (wait) {
        if (this.refreshDataTimeout != null)
          clearTimeout(this.refreshDataTimeout);

        this.refreshDataTimeout = setTimeout(() => {
          this.refreshData(false, resetPaging);
        }, 350);
        return;
      } else {
        this.refreshDataTimeout = null;
      }

      this.refreshingData = true;
      let refreshDataTime = new Date().getTime();
      this.refreshDataTime = refreshDataTime;

      if (resetPaging)
        this.page = 1;

      let url = this.getDataUrl();

      axios
        .get(url)
        .then(response => {
          // This isn't the most recent refresh request, so don't bother doing anything with it
          if (this.refreshDataTime != refreshDataTime)
            return;

          this.rawItems = response.data;
          this.total = response.headers['x-total-count'];
          this.totalMax = response.headers['x-total-count-max'];
          this.dataError = null;

          this.$emit('itemsLoaded', this.rawItems);

          this.refreshItems();

          this.refreshingData = false;
        })
        .catch(err => {
          this.rawItems = [];
          this.total = 0;
          this.dataError = 'Error loading results';
          if (err && err.response && err.response.data && err.response.data.Details)
            this.dataError += ': ' + err.response.data.Details;

          this.$emit('itemsLoaded', this.rawItems);

          this.refreshItems();

          this.refreshingData = false;
        });
    }
  },
  mounted () {
    if (this.settings.defaultSortColumn)
      this.sortColumn = this.settings.defaultSortColumn;
    if (this.settings.defaultSortOrder)
      this.sortOrder = this.settings.defaultSortOrder;

    if (this.settings.defaultLimit)
      this.limit = this.settings.defaultLimit;

    if (this.settings.loadQueryString !== false) {
      this.settings.columns.forEach(col => {
        if (this.$route.query[col.key]) {
          col.visible = true;
          this.filters[col.key] = this.$route.query[col.key];
        }
      });
    }

    this.settings.columns.forEach(col => {
      let c = this.getColumnType(col);
      if (c.init)
        this.filters[col.key] = c.init();
      else
        this.filters[col.key] = null;
    })

    if (this.defaultFilters) {
      this.settings.columns.forEach(col => {
        if (this.defaultFilters[col.key]) {
          col.visible = true;
          this.filters[col.key] = this.defaultFilters[col.key];
        }
      });
    }

    this.settings.columns.forEach(col => {
      if (col.selectOptionsSource) {
        let state = this.$store.state;
        
        if (col.selectOptionsSource.storeModule)
          state = state[col.selectOptionsSource.storeModule];

        if (col.selectOptionsSource.storeAction) {
          this.$store.dispatch((col.selectOptionsSource.storeModule ? col.selectOptionsSource.storeModule + '/' : '') + col.selectOptionsSource.storeAction).then(() => {
            col.selectOptions = state[col.selectOptionsSource.storeGetter].selectOptions;
          });
        }
        else if (col.selectOptionsSource.storeGetter) {
          col.selectOptions = state[col.selectOptionsSource.storeGetter];
        }
      }
    });

    this.refreshData();
  },
  watch: {
    defaultFilters(newValue, oldValue) {
      for (let propertyName in this.filters)
        this.filters[propertyName] = null;
      
      for (let propertyName in newValue)
        this.filters[propertyName] = newValue[propertyName];

      this.refreshData(false, true);
    },
  },
  activated () {
    this.refreshData();
  }
};
</script>