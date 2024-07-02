<script setup>
import { ref, watch, onMounted } from 'vue'
import { hocrm } from '../CRMHelper.js'

const props = defineProps({
    required: {
        required: true
    },
    modelValue: {

    },
    attName: {
    }
}
)

const value = ref(null)
const options = ref([
    {
        value: true,
        label: 'True',
    },
    {
        value: false,
        label: 'False',
    },
])

watch(value, (newValue, oldValue) => {
    if (newValue === '') {
        $emit('update:modelValue', null);
    }
    else {
        $emit('update:modelValue', newValue);
    }
})

onMounted(() => {
    if (modelValue == null || modelValue == undefined) {
        value = null
    }
    else if (modelValue) {
        value = modelValue;
    }
})
</script>
<template>
    <div style="display: flex; flex-wrap: nowrap; flex-direction: row; margin: 1em;">
        <p>{{ attName }}</p>
        <el-select v-model="value" placeholder="Select" clearable style="width: 240px">
            <el-option v-for="item in options" :key="item.value" :label="item.label" :value="item.value" />
        </el-select>
        <el-icon size="2em" :color="required && modelValue == null ? '#F56C6C' : '#67C23A'">
            <SuccessFilled />
        </el-icon>
    </div>
</template>