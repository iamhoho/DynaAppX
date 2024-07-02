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

var value = ref(null)
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
        this.$emit('update:modelValue', null);
    }
    else {
        this.$emit('update:modelValue', newValue);
    }
})

onMounted(() => {
    if (props.modelValue == null || props.modelValue == undefined) {
        value = null
    }
    else if (props.modelValue) {
        value = props.modelValue;
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