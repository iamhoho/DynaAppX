<script setup>
import { ref, watch } from 'vue'
import { hocrm } from '../CRMHelper.js'

const props = defineProps({
    required: {
        required: true
    },
    disabled: {
        required: true
    },
    modelValue: {
    }
})

var selectedName = ref("")
var selectedItem = ref(props.modelValue)

watch(selectedItem, (newValue, oldValue) => {
    selectedName = newValue?.DisplayName?.UserLocalizedLabel?.Label;
    this.$emit('update:modelValue', newValue);
})

function onChange(item) {
    selectedItem = null;
}

function handleSelect(item) {
    selectedName = item?.DisplayName?.UserLocalizedLabel?.Label;
    selectedItem = item;
}

function querySearch(queryString, cb) {
    const results = queryString
        ? hocrm.getEntityDefinitions().filter(createFilter(queryString))
        : hocrm.getEntityDefinitions();
    cb(results);
}

function createFilter(queryString) {
    return (item) => {
        return (
            item.DisplayName?.UserLocalizedLabel?.Label?.toLowerCase().indexOf(queryString.toLowerCase()) == 0 ||
            item.LogicalName.toLowerCase().indexOf(queryString.toLowerCase()) == 0 ||
            item.ObjectTypeCode.toString().toLowerCase().indexOf(queryString.toLowerCase()) == 0
        )
    }
}
</script>
<template>
    <div style="display: flex; flex-wrap: nowrap; flex-direction: row; margin: 1em;">
        <el-autocomplete :disabled=disabled v-model="selectedName" :fetch-suggestions="querySearch"
            popper-class="my-autocomplete" placeholder="Select an entity" @select="handleSelect" @change="onChange"
            style="--el-input-border-color: red;">
            <template #suffix>
                <el-icon class="el-input__icon">element-plus --save
                    <Search />
                </el-icon>
            </template>
            <template #default="{ item }">
                <span>{{ item.DisplayName?.UserLocalizedLabel?.Label }} &nbsp;&nbsp;&nbsp; {{ item.LogicalName }}</span>
            </template>
        </el-autocomplete>
        <el-icon size="2em" :color="required && selectedItem == null ? '#F56C6C' : '#67C23A'">
            <SuccessFilled />
        </el-icon>
    </div>
    <div>
    </div>
</template>