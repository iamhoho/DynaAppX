<script setup>
import { ref, watch } from 'vue'
import { daxHelper } from '../../daxHelper.js'

const props = defineProps({
    required: {
        required: true
    },
    disabled: {
        required: true
    },
    modelValue: {
    },
    labelName: {
    },
    attributeName: {
    }
})

const selectedName = ref("")
const selectedItem = ref(props.modelValue)

const emit = defineEmits(['update:modelValue']);

watch(() => selectedItem.value, (newValue, oldValue) => {
    selectedName.value = newValue?.DisplayName?.UserLocalizedLabel?.Label;
    emit('update:modelValue', newValue);
})

function onChange(item, aa) {
    selectedItem.value = null;
}

function handleSelect(item, aa) {
    selectedItem.value = item;
}

function querySearch(queryString, cb) {
    const results = queryString
        ? daxHelper.getEntityDefinitions().filter(createFilter(queryString))
        : daxHelper.getEntityDefinitions();
    cb(results);
}

function createFilter(queryString) {
    return (item) => {
        let result = (
            item.DisplayName?.UserLocalizedLabel?.Label?.toLowerCase().indexOf(queryString.toLowerCase()) > -1 ||
            item.LogicalName.toLowerCase().indexOf(queryString.toLowerCase()) == 0 ||
            item.ObjectTypeCode.toString().toLowerCase().indexOf(queryString.toLowerCase()) > -1
        )
        return result;
    }
}
</script>
<template>
    <div style="display: flex; flex-wrap: nowrap; flex-direction: row; margin: 1em; align-items: center;">
        <div class="controlLabel">
            <p>{{ labelName }}</p>
            <p v-if="attributeName">{{ attributeName }}</p>
        </div>
        <el-autocomplete :disabled=disabled v-model="selectedName" :fetch-suggestions="querySearch"
            popper-class="my-autocomplete" placeholder="Select an entity" @select="handleSelect" @change="onChange"
            style="--el-input-border-color: red;">
            <template #suffix>
                <el-icon class="el-input__icon">
                    <Search />
                </el-icon>
            </template>
            <template #default="{ item }">
                <span>{{ item.DisplayName?.UserLocalizedLabel?.Label }} &nbsp;&nbsp;&nbsp; {{ item.LogicalName }}</span>
            </template>
        </el-autocomplete>
        <el-icon class="requiredIcon" style="margin-left: 5px;" size="2em">
            <WarningFilled color="#F56C6C" v-if="required && selectedItem == null" />
            <SuccessFilled color="#67C23A" v-else />
        </el-icon>
    </div>
    <div>
    </div>
</template>