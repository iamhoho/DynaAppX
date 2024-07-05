<script setup>
import { ref, watch, onMounted } from 'vue'

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

const value = ref(props.modelValue)

const emit = defineEmits(['update:modelValue']);

watch(() => value.value, (newValue, oldValue) => {
    emit('update:modelValue', newValue);
})

onMounted(() => {

})
</script>
<template>
    <div style="display: flex; flex-wrap: nowrap; flex-direction: row; margin: 1em;">
        <p>{{ attName }}</p>
        <el-date-picker format="YYYY-MM-DDTHH:mm:ssZ" v-model="value" type="datetime" placeholder="Select date and time"
            style="width:250px" />
        <el-tooltip placement="top" effect="light">
            <template #content>The selected time zone is the same as the time zone of the current pc.</template>
            <el-icon>
                <Warning />
            </el-icon>
        </el-tooltip>
        <el-icon size="2em" :color="required && (modelValue == null || modelValue == '') ? '#F56C6C' : '#67C23A'">
            <SuccessFilled />
        </el-icon>
    </div>
</template>